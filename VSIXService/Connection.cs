using System.Net.Sockets;
using VSIXService.Helpers;

namespace VSIXService;

/// <summary>
/// tcp连接,只用一个监听对象,用了静态成员
/// </summary>
internal static class Connection
{
    /// <summary>
    /// 监听
    /// </summary>
    private static TcpListener Listener;

    /// <summary>
    /// 控制token用于是否继续等待请求
    /// </summary>
    private static CancellationTokenSource CancelTokenForAcceptTcpClient;
    /// <summary>
    /// tcp连接是否已取消
    /// </summary>
    private static bool IsCancel;

    /// <summary>
    /// 打开连接,开始提供服务
    /// </summary>
    public static async Task Start()
    {
        try
        {
            CancelTokenForAcceptTcpClient = new();
            Listener = new(Config.iPAddress, Config.port);
            Listener.Start();
            //
            while (!IsCancel)
            {
                LogHelp.SrvLog("监听循环等待处理链接中...");
#if DEBUG
                Console.WriteLine("监听循环等待处理链接中");
#endif
                TcpClient client = await Listener.AcceptTcpClientAsync(CancelTokenForAcceptTcpClient.Token);
                new Worker(client);
            }
        }
        catch (Exception e)
        {
            // 等待请求发生异常,需要重新启动程序
            // 通常是关闭服务引发(在控制台按ctrl+c,在windos服务界面点击停止),Token信号执行取消,导致监听程序停止.
            string errMsg = $"Connection连接发生异常: [{e.Message}] VSIXService服务需要重启!";
            LogHelp.SrvLog(errMsg);
#if DEBUG
            Console.WriteLine(errMsg);
#endif
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public static void Stop()
    {
        // 退出等待请求
        CancelTokenForAcceptTcpClient.Cancel();
        // 退出循环
        IsCancel = true;
        // 停止监听
        Listener.Stop();
    }
}
