using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VSIXService;

IHost host = new HostBuilder()
    //.UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<MainService>();
    })
    .Build();
host.Run();
// sc.exe create "AAA EXE Service" binpath="D:\Mirror\Project_git\WebFilePublishVSIX\VSIXService\bin\Debug\net6.0\VSIXService.exe"