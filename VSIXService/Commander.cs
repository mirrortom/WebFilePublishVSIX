using System.Reflection;
using System.Runtime.Intrinsics.Arm;
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
    /// 在服务启动时运行一次.
    /// 作用:加载命令目录
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
        if (!Cmds.TryGetValue(key, out IFun? cmdFun))
        {
            content.ResultCode = 0;
            content.Result = "无效命令!";
            return;
        }

        cmdFun.Run(content);
    }

    /// <summary>
    /// 在此统一注册命令
    /// </summary>
    private static void LoadCommands()
    {
        // 找出服务程序的程序集中所有实现了IFun的类
        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetInterface(nameof(IFun)) != null);

        foreach (Type t in types)
        {
            IFun obj = Activator.CreateInstance(t) as IFun;
            if (Cmds.ContainsKey(obj.Id))
            {
                throw new VSIXServiceException($"服务启动失败,命令 ({obj.Id}) 重复!");
            }
            Cmds.Add(obj.Id, obj);
        }
        LogHelp.SrvLog($"VSIXService服务已装载命令总数 : ({Cmds.Count})");
#if DEBUG
        Console.WriteLine($"VSIXService服务已装载命令总数 : ({Cmds.Count})");
#endif
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