using RazorService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            string extName = Path.GetExtension(src[i]).ToLower();
            string txt = File.ReadAllText(src[i]);
            string outTxt;
            // 根据扩展名选择输出方法
            if (extName == ".cshtml")
            {
                outTxt = RazorOut(txt, searchDirs, model, miniTag);
                // 将.cshtml改为.html
                target[i] = $"{target[i][0..target[i].LastIndexOf('.')]}.html";
            }
            else
            {
                outTxt = FileOut(txt, extName, miniTag);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(target[i]));
            File.WriteAllText(target[i], outTxt, Encoding.UTF8);
            info.AppendLine($"--已发布到{target[i]}");
        }
        content.ResultCode = 1;
    }

    /// <summary>
    /// js/css/html是否压缩输出,其它页面原样返回
    /// </summary>
    /// <param name="text"></param>
    /// <param name="ext"></param>
    /// <param name="miniTag"></param>
    /// <returns></returns>
    private string FileOut(string text, string ext, int miniTag)
    {
        // 判断压缩结果并返回压缩文本
        string miniResult(string mini)
        {
            string tag = ext[1..];
            if (mini == null)
            {
                info.AppendLine($"  压缩{tag}失败,请检查{tag}文件语法错误!将直接输出.");
                return text;
            }
            else
            {
                info.AppendLine($"  已压缩{tag}.");
                return mini;
            }
        }

        // js/css/html是否压缩
        if (ext == ".js" && new int[] { 4, 5, 6, 7 }.Contains(miniTag))
        {
            string js = Minifier.Js(text);
            return miniResult(js);
        }
        // css
        if (ext == ".css" && new int[] { 2, 3, 6, 7 }.Contains(miniTag))
        {
            string css = Minifier.Css(text);
            return miniResult(css);
        }
        // html
        if ((ext == ".html" || ext == ".htm") && new int[] { 1, 3, 5, 7 }.Contains(miniTag))
        {
            string html = Minifier.Html(text);
            return miniResult(html);
        }
        // 其它页面直接返回
        return text;
    }

    /// <summary>
    /// razor页面编译输出
    /// </summary>
    private string RazorOut(string text, string[] searchDirs, dynamic model, int miniTag)
    {
        // build razor
        // razor引用页搜索目录
        if (searchDirs.Length > 0)
        {
            RazorCfg.AddSearchDirs(searchDirs);
        }
        string html = RazorServe.RunTxt(text, model ?? null);
        info.AppendLine("  已编译Razor.");
        return FileOut(html, ".html", miniTag);
    }
}
