using RazorService;
using System.Text;
using VSIXService.Helpers;

namespace VSIXService.cmds;

internal class BuildRazor : IFun
{
    public int Id => 2;

    public string Desc => "编译razor文件.json参数:{path:\"razor文件路径(必须)\",searchDirs:\"razor文件搜索全路径名,多个|线分割.razor引用了其它页面时,必须指定.\",targetPath:\"输出文件全路径名,省略时返回生成文本.\",model:{数据实体}}";

    public void Run(WorkContent content)
    {
        if (string.IsNullOrWhiteSpace(content.ParaString))
            throw new VSIXServiceException("参数缺少!");
        //
        content.ParaDynamic = Helps.JSONToDynamic(content.ParaString);
        string path = content.ParaDynamic.path;
        if (string.IsNullOrWhiteSpace(path))
            throw new VSIXServiceException("path参数缺少!");
        path = path.Trim();
        // razor引用页搜索目录 每次编译时,可以选不同位置
        if (content.ParaDynamic.ContainsKey("searchDirs"))
        {
            RazorCfg.AddSearchDirs(content.ParaDynamic.searchDirs.ToString().Split('|'));
        }
        // 编译razor
        string html = RazorServe.Run(path, content.ParaDynamic.model ?? null);
        if (string.IsNullOrWhiteSpace(html))
            throw new VSIXServiceException("razor编译失败!");
        // 如果传了目标文件路径
        if (content.ParaDynamic.ContainsKey("targetPath"))
        {
            string targetFilePath = content.ParaDynamic.targetPath.ToString();
            File.WriteAllText(targetFilePath, html, Encoding.UTF8);
            content.Result = $"已生成到文件: {targetFilePath}";
        }
        else
        {
            content.Result = html;
        }
        content.ResultCode = 1;
    }
}