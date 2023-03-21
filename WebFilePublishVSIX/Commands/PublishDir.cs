using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebFilePublishVSIX;

/// <summary>
/// Command 发布选中目录
/// </summary>
[Command(PackageIds.groupIdDir)]
internal sealed class PublishDir : BaseCommand<PublishDir>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        // 右键点击后选中的文件夹(一般是一个,但也可以多个)
        var selectedDirs = await ProjectHelpers.GetSelectedItemPathsAsync();
        if (selectedDirs.Count() == 0)
        {
            OutPutInfo.Info("未选中文件夹!"); return;
        }

        // 当前活动项目路径
        Project activeProj = await VS.Solutions.GetActiveProjectAsync();
        if (activeProj == null)
        {
            OutPutInfo.Info("未选中WEB项目"); return;
        }
        // 建立发布配置对象
        EnvVar.ProjectDir = Path.GetDirectoryName(activeProj.FullPath);
        //string res = PublishHelpers.CreatePublishCfg();
        //if (res != null)
        //{
        //    OutPutInfo.Info(res); return;
        //}

        // 取得要发布的文件路径
        List<string> srcfiles = new();
        foreach (string dirPath in selectedDirs)
        {
            // 选中的可能是文件和目录,如果是目录才取出其中文件和子目录文件
            if (Directory.Exists(dirPath))
                srcfiles.AddRange(Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories));
        }
        if (srcfiles.Count == 0)
        {
            OutPutInfo.Info("至少选择一个文件夹!");
            return;
        }
#if DEBUG
        OutPutInfo.Info(string.Join(" , ", srcfiles));
        return;
#endif
        // 发布处理
        //Task.Factory.StartNew(() =>
        //{
        //    string resinfo = PublishHelpers.PublishFiles(srcfiles);
        //    if (resinfo != null)
        //    {
        //        OutPutInfo.Info(resinfo);
        //    }
        //});
    }
}