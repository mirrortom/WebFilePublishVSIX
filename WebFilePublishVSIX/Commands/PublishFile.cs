using System.IO;
using System.Linq;
using WebFilePublishVSIX.Helpers;
using WebFilePublishVSIX.Publish;

namespace WebFilePublishVSIX.Commands;

/// <summary>
/// Command handler
/// </summary>
[Command(PackageIds.cmdidPublishFile)]
internal sealed class PublishFile : BaseCommand<PublishFile>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        // 发布处理
        await new Publisher(CmdTypes.PublishFile).RunAsync();
    }
}