namespace VSIXService;

/// <summary>
/// 每个功能是一个命令,实现此接口
/// </summary>
internal interface IFun
{
    /// <summary>
    /// 命令唯一编号.一个整数,不能重复.
    /// </summary>
    internal int Id { get; }

    /// <summary>
    /// 命令功能描述
    /// </summary>
    internal string Desc { get; }

    /// <summary>
    /// 运行命令
    /// </summary>
    internal void Run(WorkContent content);
}