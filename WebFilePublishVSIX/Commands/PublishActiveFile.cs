using WebFilePublishVSIX.Helpers;
using WebFilePublishVSIX.Publish;

namespace WebFilePublishVSIX.Commands;

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
        // 发布处理
        await new Publisher(CmdTypes.PublishActiveFile).RunAsync();
    }
}