using RazorService;
using System.IO;
using System.Text;
using VSIXService.Helpers;

namespace VSIXService.cmds;

internal class ForVSIX : IFun
{
    public int Id => 3;

    public string Desc => "为WebFilePublishVSIX扩展项目执行任务.json参数:{SrcFiles:[],TargetFiles:[],SearchDirs:[],Model:{},MiniOutput:7}";
    private WorkContent content;
    private StringBuilder info;

    public void Run(WorkContent workcontent)
    {
        content = workcontent;
        info = new StringBuilder();
        //
        CheckPara();
        // publish file
        Publish();
        // return
        content.Result = info.ToString();
    }

    private void CheckPara()
    {
        if (string.IsNullOrWhiteSpace(content.ParaString))
            throw new VSIXServiceException("参数缺少!");
        //
        content.ParaDynamic = Helps.JSONToDynamic(content.ParaString);
        if (content.ParaDynamic.SrcFiles == null)
            throw new VSIXServiceException("SrcFiles参数缺少!");
        if (content.ParaDynamic.TargetFiles == null)
            throw new VSIXServiceException("TargetFiles参数缺少!");
    }

    private void Publish()
    {
        // data init
        string[] src = content.ParaDynamic.SrcFiles.ToObject<string[]>();
        string[] target = content.ParaDynamic.TargetFiles.ToObject<string[]>();
        string[] searchDirs = content.ParaDynamic.SearchDirs.ToObject<string[]>();
        int miniTag = content.ParaDynamic.MiniOutput;
        dynamic model = content.ParaDynamic.Model;

        // output
        int count = src.Length;
        info.AppendLine($"文件总数: [{count}]");
        for (int i = 0; i < count; i++)
        {
            info.AppendLine($"{i + 1}.发布:{src[i]}");

            string itemSrcPath = src[i];
            string itemTargetPath = target[i];
            // 输出文件
            FileOutput(itemSrcPath, itemTargetPath, miniTag, searchDirs, model);
        }
        content.ResultCode = 1;
    }

    /// <summary>
    /// 判断文件类型,处理后输出文件
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="targetPath"></param>
    /// <param name="miniTag"></param>
    /// <param name="searchDirs"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private void FileOutput(string srcPath, string targetPath, int miniTag, string[] searchDirs, dynamic model)
    {
        // 扩展名
        string extName = Path.GetExtension(srcPath).ToLower();
        // 目标路径目录,不存在时建立
        string dir = Path.GetDirectoryName(targetPath);
        Directory.CreateDirectory(dir);
        // razor
        if (extName == ".cshtml")
        {
            // .cshtml文件输出时,扩展名改为.html
            string newTarget = $"{targetPath[0..targetPath.LastIndexOf('.')]}.html";
            RazorOut(srcPath, newTarget, searchDirs, model, miniTag);
            info.AppendLine($"--已发布到{newTarget}");
            return;
        }
        // js/css/html
        if (extName == ".css" || extName == ".html" || extName == ".js")
        {
            HtmlCssJsOut(srcPath, targetPath, miniTag, extName);
            info.AppendLine($"--已发布到{targetPath}");
            return;
        }
        // other
        File.Copy(srcPath, targetPath, true);
        info.AppendLine($"--已发布到{targetPath}");
    }

    /// <summary>
    /// js/css/html是否压缩输出
    /// </summary>
    /// <param name="src"></param>
    /// <param name="target"></param>
    /// <param name="miniTag"></param>
    /// <param name="ext"></param>
    private void HtmlCssJsOut(string src, string target, int miniTag, string ext)
    {
        string text = File.ReadAllText(src);
        string mini = null;
        // js/css/html是否压缩
        if (ext == ".js" && new int[] { 4, 5, 6, 7 }.Contains(miniTag))
        {
            mini = Minifier.Js(text);
            MiniInfo(mini, ext);
        }
        // css
        else if (ext == ".css" && new int[] { 2, 3, 6, 7 }.Contains(miniTag))
        {
            mini = Minifier.Css(text);
            MiniInfo(mini, ext);
        }
        // html
        else if ((ext == ".html" || ext == ".htm") && new int[] { 1, 3, 5, 7 }.Contains(miniTag))
        {
            mini = Minifier.Html(text);
            MiniInfo(mini, ext);
        }
        //
        File.WriteAllText(target, mini ?? text, Encoding.UTF8);
    }

    /// <summary>
    /// razor页面编译输出
    /// </summary>
    private void RazorOut(string src, string target, string[] searchDirs, dynamic model, int miniTag)
    {
        // build razor
        // razor引用页搜索目录
        if (searchDirs.Length > 0)
        {
            RazorCfg.SetSearchDirs(searchDirs);
        }
        string html = RazorServe.Run(src, model ?? null);
        info.AppendLine("  已编译Razor.");
        if (new int[] { 1, 3, 5, 7 }.Contains(miniTag))
        {
            string mini = Minifier.Html(html);
            MiniInfo(mini, ".html");
            if (mini != null)
            {
                File.WriteAllText(target, mini, Encoding.UTF8);
                return;
            }
        }

        File.WriteAllText(target, html, Encoding.UTF8);
    }

    // html/js/css压缩结果信息提示
    private void MiniInfo(string mini, string ext)
    {
        string tag = ext[1..];
        if (mini == null)
        {
            info.AppendLine($"  压缩{tag}失败,请检查{tag}文件语法错误!将直接输出.");
        }
        else
        {
            info.AppendLine($"  已压缩{tag}.");
        }
    }
}