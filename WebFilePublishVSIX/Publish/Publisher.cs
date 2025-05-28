using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
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
        // 发布相关基础数据(项目根目录,配置文件等)
        if (!await GetDataForPublishAsync())
        {
            await OutPutInfo.VsOutPutWindowAsync(cmdcontext.Info.ToString(), true);
            return;
        }
        // 2.计算要发布的文件
        // 根据配置文件的要求,排除不合要求的文件
        if (!await GetFileToPublishAsync())
        {
            await OutPutInfo.VsOutPutWindowAsync(cmdcontext.Info.ToString(), true);
            return;
        }

        // 3.计算输出路径
        TargetPath();

        // 4.连接,调用服务
        await RequestServeAsync();

        // 5.结果显示
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

        // 根据发布条件筛选
        FilterFiles.Run(cmdcontext);
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
        if (cmdcontext.ProjectRootDir == null)
        {
            cmdcontext.Info.AppendLine("未选择活动项目,根路径获取失败,发布已停止!");
            return false;
        }
        cmdcontext.ProjectRootDir = Help.PathSplitChar(cmdcontext.ProjectRootDir);

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
            // 1.如果配置了发布路径,优先使用
            string outPath = DistMapsRule(item, cmdcontext);
            if (outPath != null)
            {
                cmdcontext.TargetFiles.Add(Help.PathSplitChar(outPath));
                continue;
            }
            // 2.使用默认输出路径规则:源文件项目目录的根目录部分,替换成发布目录.
            string rootDir = Help.PathTrim(cmdcontext.ProjectRootDir);
            string relPath = item.Substring(rootDir.Length + 1);
            // 源文件从项目根目录起始的相对路径,如: wwwroot/xxx/yy
            // 如果配置文件中设置了源代码目录,并且存在,去掉这层目录
            if (!string.IsNullOrWhiteSpace(cmdcontext.CfgM.SourceDir))
            {
                string srcDir = Help.PathTrim(cmdcontext.CfgM.SourceDir, true);
                // 如果文件属于源代码目录下
                if (Help.IsPathInDir(item, Path.Combine(rootDir, srcDir)))
                {
                    relPath = relPath.Substring(srcDir.Length + 1);
                }
            }
            // 目标文件全路径 输出目录+相对路径
            cmdcontext.TargetFiles.Add(Help.PathSplitChar(Path.Combine(cmdcontext.CfgM.DistDir, relPath)));
        }
    }

    /// <summary>
    /// 发布路径规则
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cmdcontext"></param>
    /// <returns></returns>
    private string DistMapsRule(string path, CmdContext cmdcontext)
    {
        if (cmdcontext.CfgM.DistMaps.Count == 0)
            return null;
        // 判断path是否为配置项里的文件或属于目录
        // 配置项distMaps:{"inputFileOrDir": "outFileOrDir"}键是源文件(相对目录),值是目标目录路径
        // inputFileOrDir示例:index.html或者dir/*.*
        // outDir示例:e:/outDir或者outDir
        string rootDir = Help.PathTrim(cmdcontext.ProjectRootDir);
        string outDir = string.Empty;
        // 优先匹配文件路径
        string fileKey = cmdcontext.CfgM.DistMaps.Keys.FirstOrDefault
        (k => Help.IsPathEq(path, Path.Combine(rootDir, k)));
        if (fileKey != null)
        {
            outDir = cmdcontext.CfgM.DistMaps[fileKey];
        }

        // 再匹配目录
        if (string.IsNullOrWhiteSpace(outDir))
        {
            foreach (var item in cmdcontext.CfgM.DistMaps)
            {
                string dirKey = item.Key;
                string dirVal = item.Value;
                if (string.IsNullOrWhiteSpace(dirKey) || string.IsNullOrWhiteSpace(dirVal))
                    continue;
                // 全路径,排除非目录
                string dirPath = Help.PathTrim(Path.Combine(rootDir, dirKey).TrimEnd('*', '.', '*'));
                if (!Directory.Exists(dirPath))
                    continue;
                // 文件是属于这个目录下的
                if (Help.IsPathInDir(path, dirPath))
                {
                    outDir = dirVal;
                }
            }
        }
        if (string.IsNullOrWhiteSpace(outDir))
            return null;

        return Path.IsPathRooted(outDir) ?
        Path.Combine(outDir, Path.GetFileName(path)) :
        Path.Combine(rootDir, outDir, Path.GetFileName(path));
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