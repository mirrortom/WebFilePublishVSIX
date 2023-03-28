using Microsoft.VisualStudio.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebFilePublishVSIX.Helpers;

namespace WebFilePublishVSIX.Publish;

/// <summary>
/// 发布帮助类
/// </summary>
internal class Publisher
{
    private readonly CmdContext cmdcontext;

    public Publisher(CmdTypes cmdType)
    {
        cmdcontext = new();
        cmdcontext.CmdType = cmdType;
    }

    /// <summary>
    /// 进入发布流程
    /// </summary>
    public async Task RunAsync()
    {
        cmdcontext.Info.AppendLine("START 发布开始>>>".PadRight(50, '-'));
        cmdcontext.Info.AppendLine();

        // 1.获取数据
        //// 发布相关基础数据(项目根目录,配置文件等)
        if (!await GetDataForPublishAsync())
        {
            await OutPutInfo.VsOutPutWindowAsync(cmdcontext.Info.ToString(), true);
            return;
        }
        //// 要发布的文件
        if (!await GetFileToPublishAsync())
        {
            await OutPutInfo.VsOutPutWindowAsync(cmdcontext.Info.ToString(), true);
            return;
        }

        // 2.配置数据
        //// 输出路径计算
        TargetPath();

        // 3.连接,调用服务
        await RequestServeAsync();

        // 4.结果
        cmdcontext.Info.AppendLine("END 发布结束<<<".PadRight(50, '-'));
        await OutPutInfo.VsOutPutWindowAsync(cmdcontext.Info.ToString(), true);
    }

    #region 获取并检查数据

    /// <summary>
    /// 获取要发布的文件
    /// </summary>
    private async Task<bool> GetFileToPublishAsync()
    {
        cmdcontext.SrcFiles = new();
        // 根据不同的命令,选取要发布的文件
        if (cmdcontext.CmdType == CmdTypes.PublishActiveFile)
        {
            // 当前编辑的文件
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView != null)
            {
                string p = docView.TextBuffer.GetFileName();
                if (p != null)
                    cmdcontext.SrcFiles.Add(p);
            }
        }
        else if (cmdcontext.CmdType == CmdTypes.PublishWeb)
        {
            List<string> pathItems = await ProjectHelpers.GetItemsPathAsync();
            // 获取项目内所有文件和目录
            foreach (var item in pathItems)
            {
                // 是目录,还要获取里面的所有文件
                if (Directory.Exists(item))
                    cmdcontext.SrcFiles.AddRange(Directory.GetFiles(item, "*", SearchOption.AllDirectories));
                else if (File.Exists(item))
                    cmdcontext.SrcFiles.Add(item);
            }
        }
        else if (cmdcontext.CmdType == CmdTypes.PublishFile)
        {
            var selectedItems = await ProjectHelpers.GetSelectedItemPathsAsync();
            // 右键点击后选中的文件(一般是一个,但也可以多个)
            foreach (string dirPath in selectedItems)
            {
                // 只取文件
                if (File.Exists(dirPath))
                    cmdcontext.SrcFiles.Add(dirPath);
            }
        }
        else if (cmdcontext.CmdType == CmdTypes.PublishDir)
        {
            var selectedItems = await ProjectHelpers.GetSelectedItemPathsAsync();
            // 右键点击后选中的文件夹(一般是一个,但也可以多个)
            foreach (string dirPath in selectedItems)
            {
                // 选中的可能是文件和目录,是目录才取出其中文件和子目录文件
                if (Directory.Exists(dirPath))
                    cmdcontext.SrcFiles.AddRange(Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories));
            }
        }

        // 根据发布条件筛选
        FilterFiles();
        // 至少要有一个文件发布
        if (cmdcontext.SrcFiles.Count == 0)
        {
            cmdcontext.Info.AppendLine("没有可发布的文件,发布已停止!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 获取发布时需要的相关数据,并检查
    /// </summary>
    /// <returns></returns>
    private async Task<bool> GetDataForPublishAsync()
    {
        // 最先获取基础数据

        // 活动项目根路径,最基础数据
        cmdcontext.ProjectRootDir = await ProjectHelpers.GetActiveProjectRootDirAsync();
        cmdcontext.ProjectRootDir = Help.PathSplitChar(cmdcontext.ProjectRootDir);
        if (cmdcontext.ProjectRootDir == null)
        {
            cmdcontext.Info.AppendLine("未选择活动项目,根路径获取失败,发布已停止!");
            return false;
        }

        // 获取发布配置文件,必要数据
        ConfigM.CreatePublishCfg(cmdcontext);
        if (cmdcontext.CfgM == null)
            return false;

        // 如果输出目录是相对目录,需要加上项目根目录,成为全路径
        if (!Path.IsPathRooted(cmdcontext.CfgM.DistDir))
        {
            cmdcontext.CfgM.DistDir = Path.Combine(cmdcontext.ProjectRootDir, cmdcontext.CfgM.DistDir);
        }

        // razor页面搜索目录,加上全路径
        if (cmdcontext.CfgM.RazorSearchDirs.Length > 0)
        {
            for (int i = 0; i < cmdcontext.CfgM.RazorSearchDirs.Length; i++)
            {
                string p = cmdcontext.CfgM.RazorSearchDirs[i];
                cmdcontext.CfgM.RazorSearchDirs[i] = Help.PathSplitChar(Path.Combine(cmdcontext.ProjectRootDir, p));
            }
        }
        return true;
    }

    /// <summary>
    /// 使用预定规则将不需要发布处理的文件清除,筛选出发布文件列表.
    /// </summary>
    /// <param name="filesPath"></param>
    /// <returns></returns>
    private void FilterFiles()
    {
        List<string> files = new();
        // 此时的发布目录DistDir,必须已经是全路径
        string outDirLower = Help.PathSplitChar(cmdcontext.CfgM.DistDir).ToLower();
        string jsonCfgPathLower = Help.PathSplitChar(Path.Combine(cmdcontext.ProjectRootDir, EnvVar.PublishCfgName)).ToLower();
        // 循环所有文件,筛选
        foreach (var item in cmdcontext.SrcFiles)
        {
            // item可能为null,在调试时发现.可能是获取文件路径的接口有时候返回null,但没有发现.
            if (string.IsNullOrEmpty(item)) continue;

            string filePathLower = Help.PathSplitChar(item).ToLower();

            // 可能是个目录,要排除
            if (Directory.Exists(filePathLower))
                continue;

            // 如果文件位于发布目录下要排除掉.避免发布"发布目录里的文件".
            // 例如发布目录是默认值dist时,这是位于项目根目录下的dist文件夹,如果意外被包含进项目,就会发生此情况
            if (filePathLower.StartsWith(outDirLower))
                continue;

            // 只发布支持的扩展名
            if (cmdcontext.CfgM.AllowExts.FirstOrDefault(o => o.ToLower() == Path.GetExtension(filePathLower)) == null)
                continue;

            // 排除不允许发布的文件后缀名
            if (cmdcontext.CfgM.DenySuffix.FirstOrDefault
                (o => filePathLower.EndsWith(o.ToLower())) != null)
                continue;

            // 排除不允许发布的目录,比较文件目录,是否为禁发目录开头
            if (cmdcontext.CfgM.DenyDirs.FirstOrDefault(o =>
            {
                // 禁发目录取得全路径,再比较
                string denyDir = Help.PathSplitChar(Path.Combine(cmdcontext.ProjectRootDir, o)).ToLower();
                return filePathLower.StartsWith(denyDir);
            }) != null)
                continue;

            // 排除不允许发布的文件,比较文件全路径名
            if (cmdcontext.CfgM.DenyFiles.FirstOrDefault(o =>
            {
                // 禁发文件取得全路径,再比较
                string denyFile = Help.PathSplitChar(Path.Combine(cmdcontext.ProjectRootDir, o)).ToLower();
                return filePathLower == denyFile;
            }) != null)
                continue;

            // 不发布配置文件
            if (filePathLower == jsonCfgPathLower)
                continue;
            //
            files.Add(Help.PathSplitChar(item));
        }
        cmdcontext.SrcFiles = files;
    }

    #endregion 获取并检查数据

    #region 计算发布路径

    /// <summary>
    /// 根据要发布的源文件路径,计算出目标路径.
    /// </summary>
    /// <returns></returns>
    private void TargetPath()
    {
        cmdcontext.TargetFiles = new();
        foreach (var item in cmdcontext.SrcFiles)
        {
            // 源文件从项目根目录起始的相对路径,如: wwwroot/xxx/yy
            string rootDir = Help.PathTrim(cmdcontext.ProjectRootDir);
            string relPath = item.Substring(rootDir.Length + 1);
            // 源代码目录这一级(或者几级),去掉
            if (!string.IsNullOrWhiteSpace(cmdcontext.CfgM.SourceDir))
            {
                string srcDir = Help.PathTrim(cmdcontext.CfgM.SourceDir, true);
                // 如果是源代码目录下的,才要去掉
                if (relPath.StartsWith(srcDir, true, null))
                {
                    relPath = relPath.Substring(srcDir.Length + 1);
                }
            }

            // 目标文件全路径 输出目录+相对路径
            cmdcontext.TargetFiles.Add(Help.PathSplitChar(Path.Combine(cmdcontext.CfgM.DistDir, relPath)));
        }
    }

    #endregion

    #region 请求服务

    private async Task RequestServeAsync()
    {
        TcpClient client = null;
        NetworkStream stream = null;
        try
        {
            // 建立连接
            client = new();
            await client.ConnectAsync(IPAddress.Parse(cmdcontext.CfgM.Ip), cmdcontext.CfgM.Port);
            stream = client.GetStream();

            // 参数包装
            byte[] data = GetRequestData();

            // send
            await stream.WriteAsync(data, 0, data.Length);

            // return
            byte[] buffer = new byte[4096];
            int lastIndex = await stream.ReadAsync(buffer, 0, buffer.Length);

            // info
            cmdcontext.Info.AppendLine(buffer[0] == 1 ? "^_^服务执行成功!" : ":(服务执行失败!");
            cmdcontext.Info.AppendLine("<服务消息>");
            // \0是字符串结尾,如果在stringbuild中加入了此字符,后面再append()其它字符串就徒劳了,
            // ToString()时被限制在\0位置,\0后面的字符不出来.
            // socket传字符串就会带来\0这个问题,要去掉这个字符. lastIndex-1
            string srvmsg = Encoding.UTF8.GetString(buffer.Skip(1).Take(lastIndex - 1).ToArray());
            cmdcontext.Info.AppendLine(srvmsg);
        }
        catch (Exception e)
        {
            cmdcontext.Info.AppendLine($"RequestServeAsync()异常,请检查插件服务! {e.Message}");
        }
        finally
        {
            stream?.Close();
            client?.Close();
        }
    }

    /// <summary>
    /// 打包参数,按协议格式.
    /// 协议:第1个字节是命令是否成功,后面是数据
    /// </summary>
    /// <returns></returns>
    private byte[] GetRequestData()
    {
        List<byte> data = new();
        // 命令编号 3 ,对应VSIXService.cmds.ForVSIX
        data.Add(3);

        // json数据
        dynamic p = new ExpandoObject();
        p.SrcFiles = cmdcontext.SrcFiles;
        p.TargetFiles = cmdcontext.TargetFiles;
        // razor搜索路径,默认项目根路径
        p.SearchDirs =
            cmdcontext.CfgM.RazorSearchDirs.Concat(new string[] { cmdcontext.ProjectRootDir }).ToArray();
        p.Model = cmdcontext.CfgM.RazorModel;
        p.MiniOutput = cmdcontext.CfgM.MiniOutput;
        //
        string json = JsonConvert.SerializeObject(p);
        data.AddRange(Encoding.UTF8.GetBytes(json));
        return data.ToArray();
    }

    #endregion 请求服务
}