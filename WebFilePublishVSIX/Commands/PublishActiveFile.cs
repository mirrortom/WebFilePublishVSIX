using System.Collections.Generic;
using System.IO;

namespace WebFilePublishVSIX;

/// <summary>
/// Command 发布当前活动文件
/// </summary>
[Command(PackageIds.cmdidPublishActiveFile)]
internal sealed class PublishActiveFile : BaseCommand<PublishActiveFile>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        // 发布当前处于活动状态的1个文件 如果没有活动文件,不动作
        DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
        if (docView?.Document == null)
        {
            OutPutInfo.Info("未找到激活的文件");
            return;
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
        List<string> srcfiles = new List<string>
        {
            docView.FilePath
        };
#if DEBUG
        OutPutInfo.Info(string.Join(" , ", srcfiles));
        return;
#endif
        // 发布处理
        //Task.Run(() =>
        //  {
        //      string resinfo = PublishHelpers.PublishFiles(srcfiles);
        //      if (resinfo != null)
        //      {
        //          OutPutInfo.Info(resinfo);
        //      }
        //  });
    }
}