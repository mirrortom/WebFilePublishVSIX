using System.Net;

namespace WebFilePublishVSIX;

internal static class EnvVar
{
    /// <summary>
    /// 插件名字.
    /// </summary>
    internal const string Name = "web publiser vsix";

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

    /// <summary>
    /// 当前要发布的项目的根路径.例如"e:/vs2017/myproject"(正斜杠,后面无/)
    /// </summary>
    internal static string ProjectDir;

}