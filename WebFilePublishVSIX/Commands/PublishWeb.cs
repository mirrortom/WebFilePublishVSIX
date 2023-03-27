using Microsoft.Internal.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebFilePublishVSIX.Helpers;
using WebFilePublishVSIX.Publish;

namespace WebFilePublishVSIX.Commands;

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
        // 发布处理
        await new Publisher(CmdTypes.PublishWeb).RunAsync();
    }
}
