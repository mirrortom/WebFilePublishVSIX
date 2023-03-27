using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebFilePublishVSIX.Helpers;
using WebFilePublishVSIX.Publish;

namespace WebFilePublishVSIX.Commands;

/// <summary>
/// Command 发布选中目录
/// </summary>
[Command(PackageIds.cmdidPublishDir)]
internal sealed class PublishDir : BaseCommand<PublishDir>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        /// 发布处理
        await new Publisher(CmdTypes.PublishDir).RunAsync();
    }
}