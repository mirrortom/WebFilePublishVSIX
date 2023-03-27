using Newtonsoft.Json;
using System.Dynamic;
using System.IO;
using System.Text;
using WebFilePublishVSIX.Helpers;

namespace WebFilePublishVSIX.Publish;

internal class ConfigM
{
    /// <summary>
    /// 服务地址
    /// </summary>
    public string Ip = "127.0.0.1";

    /// <summary>
    /// 服务端口
    /// </summary>
    public int Port = 50_015;

    /// <summary>
    /// 源文件目录名(例如:src 相对路径,相对于项目根目录,这层目录不会发布,例如src/a/a.txt发布后是/a/a.txt)
    /// </summary>
    public string SourceDir = "src";

    /// <summary>
    /// 发布目录名(例如: 'dist' 或 'd:/pubdir' 相对路径或绝对路径)
    /// </summary>
    public string DistDir = "dist";

    /// <summary>
    /// 允许发布的文件扩展名列表
    /// </summary>
    public string[] AllowExts = { ".htm", ".html", ".cshtml", ".config", ".js", ".css", ".json", ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".woff", ".woff2", ".ttf", ".svg", ".eot", ".otf" };

    /// <summary>
    /// 不允许发布的文件后缀名
    /// </summary>
    public string[] DenySuffix = { "layout.cshtml", "part.cshtml", "part.js", "part.css" };

    /// <summary>
    /// 对js,css,html压缩输出(0=不压缩,7=都压缩(默认). 1=html,2=css,4=js)
    /// </summary>
    public int MiniOutput = 7;

    /// <summary>
    /// 不允许发布的文件(例如:bundleconfig.json data/data.mdb 相对路径的文件名,相对于项目根目录)
    /// </summary>
    public string[] DenyFiles = { "bundleconfig.json", "compilerconfig.json" };

    /// <summary>
    /// 不允许发布的目录(例如:cfg 相对路径,相对于项目根目录)
    /// </summary>
    public string[] DenyDirs = { };
    /// <summary>
    /// razor部分页搜索路径
    /// </summary>
    public string[] RazorSearchDirs = { };
    /// <summary>
    /// razor model数据
    /// </summary>
    public dynamic RazorModel = new ExpandoObject();

    /// <summary>
    /// 根据发布配置文件生成发布配置对象.配置文件publish.json放在项目根目录下.
    /// 如果没有配置文件,将使用默认配置,并在项目根目录下生成publish.json配置文件
    /// 如果指定的配置文件不是有效的json,返回出错提示.
    /// 如果配置文件的值无效,将置为默认值.
    /// 失败时返回null
    /// </summary>
    /// <returns></returns>
    internal static void CreatePublishCfg(CmdContext cmdContext)
    {
        try
        {
            string cfgPath = Path.Combine(cmdContext.ProjectRootDir, EnvVar.PublishCfgName);

            // 没有配置文件时,在项目根目录下生成publish.json配置文件
            if (!File.Exists(cfgPath))
            {
                var cfg = new ConfigM();
                File.WriteAllText(cfgPath, ConfigM.CreateJson(cfg), Encoding.UTF8);
                cmdContext.CfgM = cfg;
                return;
            }
            // 已有
            cmdContext.CfgM = JsonConvert.DeserializeObject<ConfigM>(File.ReadAllText(cfgPath));
        }
        catch (Exception e)
        {
            cmdContext.Info.AppendLine($"publish.json生成失败,发布已停止! 异常消息: {e.Message}");
            cmdContext.CfgM = null;
        }
    }

    /// <summary>
    /// 生成默认json配置文件.以json格式文本形式返回.
    /// 在项目没有配置文件时执行一次.
    /// </summary>
    private static string CreateJson(ConfigM cfg)
    {
        StringBuilder sb = new();
        //
        sb.AppendLine("{");

        //
        sb.AppendLine("  // 服务Ip地址 127.0.0.1");
        string k = nameof(cfg.Ip);

        k = Help.StartLower(k);
        string v = cfg.Ip;
        sb.AppendLine($"  \"{k}\": \"{v}\",");

        //
        sb.AppendLine("  // 服务端口 50_015");
        k = nameof(cfg.Port);
        k = Help.StartLower(k);
        v = cfg.Port.ToString();
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // 源文件目录名(例如:src 相对路径,相对于项目根目录,这层目录不会发布,例如src/a/a.txt发布后是/a/a.txt)");
        k = nameof(cfg.SourceDir);
        k = Help.StartLower(k);
        v = cfg.SourceDir;
        sb.AppendLine($"  \"{k}\": \"{v}\",");

        //
        sb.AppendLine("  // 发布目录名(例如: 'dist' 或 'd:/pubdir' 相对路径或绝对路径)");
        k = nameof(cfg.DistDir);
        k = Help.StartLower(k);
        v = cfg.DistDir;
        sb.AppendLine($"  \"{k}\": \"{v}\",");

        //
        sb.AppendLine("  // 对js,css,html压缩输出(0=不压缩,7=都压缩. 1=html,2=css,4=js)");
        k = nameof(cfg.MiniOutput);
        k = Help.StartLower(k);
        v = cfg.MiniOutput.ToString();
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // 允许发布的文件扩展名,例: .html(.号开头)");
        k = nameof(cfg.AllowExts);
        k = Help.StartLower(k);
        v = $"[ \"{string.Join("\", \"", cfg.AllowExts)}\" ]";
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // 不允许发布的文件后缀名");
        k = nameof(cfg.DenySuffix);
        k = Help.StartLower(k);
        v = $"[ \"{string.Join("\", \"", cfg.DenySuffix)}\" ]";
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // 不允许发布的文件(例如:bundleconfig.json data/data.mdb 相对路径的文件名,相对于项目根目录)");
        k = nameof(cfg.DenyFiles);
        k = Help.StartLower(k);
        v = "[]";
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // 不允许发布的目录(例如:cfg 相对路径,相对于项目根目录)");
        k = nameof(cfg.DenyDirs);
        k = Help.StartLower(k);
        v = "[]";
        sb.AppendLine($"  \"{k}\": {v},");

        //
        sb.AppendLine("  // razor部分页搜索路径,razor页面引用的母版页部分页在此目录下查找");
        k = nameof(cfg.RazorSearchDirs);
        k = Help.StartLower(k);
        v = "[]";
        sb.AppendLine($"  \"{k}\": {v},");
        
        //
        sb.AppendLine("  // razor model数据,一个json键值对例如{ name : 'mirror' , ... },将作为匿名对象作为Model传给cshtml文件,调用方法@Model.name");
        k = nameof(cfg.RazorModel);
        k = Help.StartLower(k);
        v = "{}";
        sb.AppendLine($"  \"{k}\": {v}");

        //
        sb.AppendLine("}");
        //
        return sb.ToString();
    }
}