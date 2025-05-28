using Microsoft.Extensions.Hosting;
using RazorService;
using VSIXService.Helpers;

namespace VSIXService;

internal sealed class MainService : IHostedService
{
    // string testFile = AppContext.BaseDirectory;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogHelp.SrvLog("Start VSIXService服务开启!");
        Commander.Start();
        Connection.Start();
        RazorCfg.StartTimer();
#if DEBUG
        Console.WriteLine("开启服务VSIXService!(控制台测试)");
#endif
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LogHelp.SrvLog("End VSIXService服务关闭!");
        Connection.Stop();
        RazorCfg.StopTimer();
#if DEBUG
        Console.WriteLine("关闭服务VSIXService!(控制台测试)");
#endif
        return Task.CompletedTask;
    }
}