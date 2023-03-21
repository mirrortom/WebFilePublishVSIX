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
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        LogHelp.SrvLog("End VSIXService服务关闭!");
        Connection.Stop();
        RazorCfg.StopTimer();
        return Task.CompletedTask;
    }
}