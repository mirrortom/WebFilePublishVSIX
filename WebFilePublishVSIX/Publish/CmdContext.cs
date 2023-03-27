using System.Collections.Generic;
using System.Text;
using WebFilePublishVSIX.Helpers;

namespace WebFilePublishVSIX.Publish;

/// <summary>
/// 命令工作对象,在命令开始时建立
/// </summary>
internal class CmdContext
{
    public CmdContext()
    {
        this.Info = new();
    }
    /// <summary>
    /// 命令类型
    /// </summary>
    internal CmdTypes CmdType { get; set; }
    /// <summary>
    /// 要发布的项目的根路径.
    /// </summary>
    internal string ProjectRootDir { get; set; }

    /// <summary>
    /// 要发布的文件(全路径名)
    /// </summary>
    internal List<string> SrcFiles { get; set; }

    /// <summary>
    /// 目标文件,与SrcFiles下标对应(全路径名)
    /// </summary>
    internal List<string> TargetFiles { get; set; }

    /// <summary>
    /// 消息字符串缓存
    /// </summary>
    internal StringBuilder Info { get; set; }

    /// <summary>
    /// 发布配置文件对象(用于公用)
    /// </summary>
    internal ConfigM CfgM { get; set; }
}