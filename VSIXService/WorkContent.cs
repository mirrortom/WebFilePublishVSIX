namespace VSIXService;

internal class WorkContent
{
    /// <summary>
    /// 命令编号
    /// </summary>
    public int CmdId;

    /// <summary>
    /// 字符串形式参数,可以是json格式或者querystring格式
    /// </summary>
    public string ParaString { get; set; } = string.Empty;

    /// <summary>
    /// 动态类型形式参数
    /// </summary>
    public dynamic ParaDynamic { get; set; }

    /// <summary>
    /// 结果数据
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// 任务执行结果1=成功 0=失败 ...
    /// </summary>
    public byte ResultCode { get; set; }
}