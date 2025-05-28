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

// 安装为windows服务,安装前要开启.UseWindowsService().发布时选依赖就行.
// 建立
// sc.exe create "VSIXService" binpath="D:\Mirror\Project_git\WebFilePublishVSIX\VSIXService\bin\Release\net8.0\publish\win-x64\VSIXService.exe" displayname="WebFilePublishVSIX服务"
// 添加描述
// sc.exe description "VSIXService" "为本机webFilePublishVS插件提供功能"
