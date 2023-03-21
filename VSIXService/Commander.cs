using System.Reflection;
using System.Text;
using VSIXService.Helpers;

namespace VSIXService;

/// <summary>
/// 功能管理
/// </summary>
internal sealed class Commander
{
    private static SortedDictionary<int, IFun> Cmds;

    /// <summary>
    /// 在服务启动时运行一次
    /// </summary>
    internal static void Start()
    {
        // 加载应用命令
        Cmds = new();
        LoadCommands();
    }

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="content"></param>
    internal static void Execute(WorkContent content)
    {
        int key = content.CmdId;

        // 执行命令
        // 系统内命令
        switch (key)
        {
            case 0:
                CmdList(content);
                return;

            default:
                break;
        }
        // 服务命令
        if (!Cmds.ContainsKey(key))
        {
            content.ResultCode = 0;
            content.Result = "无效命令!";
            return;
        }
        //
        try
        {
            Cmds[key].Run(content);
        }
        catch (Exception e)
        {
            // 命令执行发生错误,消息要发到请求方.
            content.ResultCode = 0;
            content.Result = e.Message;
        }
    }

    /// <summary>
    /// 在此统一注册命令
    /// </summary>
    private static void LoadCommands()
    {
        // 找出服务程序的程序集中所有实现了IConsoleApp的类
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (Type t in types)
        {
            if (t.GetInterface(nameof(IFun)) != null)
            {
                IFun obj = Activator.CreateInstance(t) as IFun;
                if (Cmds.ContainsKey(obj.Id))
                {
                    throw new VSIXServiceException($"服务启动失败,命令 ({obj.Id}) 重复!");
                }
                Cmds.Add(obj.Id, obj);
            }
        }
        LogHelp.SrvLog($"VSIXService服务已装载命令总数 : ({Cmds.Count})");
    }

    /// <summary>
    /// 显示命令列表
    /// </summary>
    /// <param name="content"></param>
    private static void CmdList(WorkContent content)
    {
        StringBuilder sb = new();
        foreach (var k in Cmds.Keys)
        {
            sb.AppendLine($"{k} {Cmds[k].Desc}");
        }
        content.Result = sb.ToString();
        content.ResultCode = 1;
    }
}