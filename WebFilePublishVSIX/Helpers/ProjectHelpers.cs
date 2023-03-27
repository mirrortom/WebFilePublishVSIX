using System;
using System.Collections.Generic;
using System.IO;

namespace WebFilePublishVSIX.Helpers;

/// <summary>
/// 获取项目内资源
/// </summary>
internal static class ProjectHelpers
{
    /// <summary>
    /// 获取当前项目中的所有ProjectItem的路径(它不含bin下的内容,含项目源文件).
    /// 用于发布项目中的文件时,获取源文件路径
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    internal static async Task<List<string>> GetItemsPathAsync()
    {
        Project project = await VS.Solutions.GetActiveProjectAsync();
        List<string> files = new();
        foreach (var item in project.Children)
        {
            // 排除隐藏文件,比如obj和debug目录,通常情况下,隐藏文件不属于项目文件
            // 路径例子
            // 目录: C:\Users\Mirror\source\repos\WebApplication1\WebApplication1\wwwroot\
            // 文件: C:\Users\Mirror\source\repos\WebApplication1\WebApplication1\_Layout.cshtml
            if (item.IsNonVisibleItem)
                continue;
            if (item != null)
            {
                files.Add(item.FullPath);
            }
        }
        return files;
    }

    /// <summary>
    /// 项目资源管理窗口中,选中的: 文件/目录的全(物理)路径 ()
    /// </summary>
    /// <returns></returns>
    internal static async Task<IList<string>> GetSelectedItemPathsAsync()
    {
        SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
        List<string> paths = new();
        if (solutionExplorer is not null)
        {
            // 得到所有选中的项目,可以是解决方案(*.sln)/项目(*.csproj)/目录/文件
            var selectedItems = await solutionExplorer.GetSelectionAsync();
            foreach (SolutionItem item in selectedItems)
            {
                // 排除隐藏文件,比如obj和debug目录,通常情况下,隐藏文件不属于项目文件
                // https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/issues/401
                if (item.IsNonVisibleItem)
                    continue;
                if (item.Type == SolutionItemType.PhysicalFile || item.Type == SolutionItemType.PhysicalFolder)
                {
                    paths.Add(item.FullPath);
                }
            }
        }
        return paths;
    }

    /// <summary>
    /// 获取活动项目根目录路径
    /// </summary>
    /// <returns></returns>
    internal static async Task<string> GetActiveProjectRootDirAsync()
    {
        Project activeProj = await VS.Solutions.GetActiveProjectAsync();
        return activeProj == null ? null : Path.GetDirectoryName(activeProj.FullPath);
    }
    /// <summary>
    /// 获取活动项目编译输出路径. 一个相对于项目根目录起始的目录 \bin\Debug|Release\net6.0\
    /// </summary>
    /// <returns></returns>
    internal static async Task<string> GetActiveProjectOutBuildPathAsync()
    {
        Project activeProj = await VS.Solutions.GetActiveProjectAsync();
        string outBinDir = await activeProj.GetAttributeAsync("OutputPath");
        return outBinDir;
    }
    /// <summary>
    /// 编译当前活动项目
    /// </summary>
    /// <returns></returns>
    internal static async Task<bool> BuildProjectAsync()
    {
        Project activeProj = await VS.Solutions.GetActiveProjectAsync();
        return await activeProj.BuildAsync();
    }
}