using Microsoft.Internal.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebFilePublishVSIX;

/// <summary>
/// Command 发布项目
/// </summary>
[Command(PackageIds.cmdidPublishWeb)]
internal sealed class PublishWeb : BaseCommand<PublishWeb>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
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

        // 发布前删除发布目录下所有文件(根据配置文件的设置而执行)
        string emptyOutDir = PublishHelpers.EmptyPuslishDir();
        //if (emptyOutDir != null)
        //{
        //    OutPutInfo.Info(emptyOutDir); return;
        //}

        //
        OutPutInfo.VsOutPutWindow("<<<--发布项目-->>>" + Environment.NewLine, true);

        // 如果要发布bin目录,才编译项目
        if (PublishHelpers.JsonCfg.BuildBin == true)
        {
            // 编译项目
            bool isBuildSuccess = await activeProj.BuildAsync();
            if (isBuildSuccess == true)
            {
                // 编译后DLL文件所在目录.是一个相对于项目根目录起始的目录 // bin\Debug\net6.0\
                string outBinDir = await activeProj.GetAttributeAsync("OutputPath");
                outBinDir = outBinDir.Replace('\\', '/').Trim('/');
                // 复制dll文件到目标目录
                string resBin = PublishHelpers.PublishBin(outBinDir);
                if (resBin != null)
                {
                    OutPutInfo.Info(resBin);
                }
            }
            // 项目编译失败,停止发布
            OutPutInfo.Info("项目编译失败,发布已停止.");
            return;
        }

        // 发布项目文件
        // 取得项目中所有文件
        List<string> srcfiles = ProjectHelpers.GetItemsPath(activeProj);
        if (srcfiles.Count == 0)
        {
            OutPutInfo.Info("未发布文件,没有找到适合发布的文件.");
            return;
        }

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
