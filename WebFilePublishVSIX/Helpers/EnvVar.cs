namespace WebFilePublishVSIX.Helpers;

internal static class EnvVar
{
    /// <summary>
    /// 输出窗口标题
    /// </summary>
    internal const string Title = "<Web-Publiser-Vsix>";

    /// <summary>
    /// 输出窗口中的一个面板标识,该面板用于输出插件工作信息
    /// </summary>
    internal static Guid outPutWinPanelGuid;

    /// <summary>
    /// 输出窗口的标识
    /// </summary>
    internal static Guid outPutWindowGuid = new(WindowGuids.OutputWindow);

    /// <summary>
    /// 发布配置文件名字.这个文件放在项目根目录下
    /// </summary>
    internal const string PublishCfgName = "publish.json";
}
internal enum CmdTypes
{
    PublishActiveFile,
    PublishFile,
    PublishDir,
    PublishWeb
}