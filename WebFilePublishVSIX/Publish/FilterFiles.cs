using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebFilePublishVSIX.Helpers;

namespace WebFilePublishVSIX.Publish;

/// <summary>
/// 根据发布条件筛选
/// 使用配置文件的规则,排除不需要发布处理的文件,收集合法的发布文件列表.
/// </summary>
internal class FilterFiles
{
    /// <summary>
    /// 运行筛选 context:上下文对象
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static void Run(CmdContext context)
    {
        List<string> files = new();

        // 循环所有文件,筛选
        foreach (var item in context.SrcFiles)
        {
            bool isChecked = BaseRule(item)
            && DirPathRule(item, context)
            && ExtNameRule(item, context)
            && FileNameRule(item, context);
            if (false == isChecked)
            {
                continue;
            }
            files.Add(Help.PathSplitChar(item));
        }
        context.SrcFiles = files;
    }

    /// <summary>
    /// 基本规则
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool BaseRule(string path)
    {
        // item可能为null,在调试时发现.可能是获取文件路径的接口有时候返回null,但没有发现.
        if (string.IsNullOrEmpty(path))
            return false;

        // 可能是个目录,要排除.因为有些文件没有扩展名.
        if (Directory.Exists(path))
            return false;
        return true;
    }

    /// <summary>
    /// 文件目录路径规则
    /// </summary>
    /// <returns></returns>
    private static bool DirPathRule(string path, CmdContext context)
    {
        // 发布目录DistDir,此处必须已经是全路径
        string outDir = Help.PathSplitChar(context.CfgM.DistDir);

        // 文件位于发布目录下,要排除.避免发布"发布目录里的文件".
        // 例如发布目录是默认值dist时,这是位于项目根目录下的dist文件夹,如果意外被包含进项目,就会发生此情况
        if (path.StartsWith(outDir, true, null))
            return false;

        // 文件是否位于不允许发布的目录下.
        if (context.CfgM.DenyDirs.FirstOrDefault(o =>
        {
            // 禁发目录取得全路径,再比较
            string denyDir = Help.PathSplitChar(Path.Combine(context.ProjectRootDir, o));
            return path.StartsWith(denyDir, true, null);
        }) != null)
            return false;
        return true;
    }

    /// <summary>
    /// 文件扩展名,和后缀名规则
    /// </summary>
    /// <param name="path"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool ExtNameRule(string path, CmdContext context)
    {
        // 只发布支持的扩展名
        if (context.CfgM.AllowExts.FirstOrDefault(
        o => o.ToLower() == Path.GetExtension(path).ToLower()) == null)
            return false;

        // 排除不允许发布的文件后缀名
        if (context.CfgM.DenySuffix.FirstOrDefault
            (o => path.EndsWith(o, true, null)) != null)
            return false;
        return true;
    }

    /// <summary>
    /// 文件规则
    /// </summary>
    /// <param name="path"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool FileNameRule(string path, CmdContext context)
    {
        // 排除不允许发布的文件,比较文件全路径名
        if (context.CfgM.DenyFiles.FirstOrDefault(o =>
        {
            // 禁发文件取得全路径,再比较
            string denyFile = Help.PathSplitChar(Path.Combine(context.ProjectRootDir, o)).ToLower();
            return path.ToLower() == denyFile;
        }) != null)
            return false;

        // 不发布配置文件
        return path.ToLower() != Help.PathSplitChar(Path.Combine(context.ProjectRootDir, EnvVar.PublishCfgName)).ToLower();
    }
}
