using System.IO;
using System.Text;

namespace WebFilePublishVSIX
{
    class PublishCfgM
    {
        /// <summary>
        /// 源文件目录名(例如:src 相对路径,相对于项目根目录)
        /// </summary>
        public string SourceDir;
        /// <summary>
        /// 发布目录名(例如: 'dist' 或 'd:/pubdir' 相对路径或绝对路径)
        /// </summary>
        public string DistDir;
        /// <summary>
        /// 允许发布的文件扩展名列表
        /// </summary>
        public string[] AllowExts;
        /// <summary>
        /// 不允许发布的文件后缀名
        /// </summary>
        public string[] DenySuffix;
        /// <summary>
        /// 发布整个项目时,发布前删除发布目录所有内容
        /// </summary>
        public bool EmptyPublishDir = true;
        /// <summary>
        /// 发布整个项目时,编译项目后发布bin文件夹
        /// </summary>
        public bool BuildBin = false;
        /// <summary>
        /// 对js,css,html压缩输出(0=不压缩,7=都压缩(默认). 1=html,2=css,4=js)
        /// </summary>
        public int MiniOutput = 7;
        /// <summary>
        /// 不允许发布的文件(例如:bundleconfig.json data/data.mdb 相对路径的文件名,相对于项目根目录)
        /// </summary>
        public string[] DenyFiles;

        /// <summary>
        /// 不允许发布的目录(例如:cfg 相对路径,相对于项目根目录)
        /// </summary>
        public string[] DenyDirs;
        public dynamic GlobleVar;
        /// <summary>
        /// 检擦配置值,将空值或未设定的设为默认值,将路径的\改为/
        /// </summary>
        public void CheckVal()
        {
            if (string.IsNullOrWhiteSpace(this.SourceDir))
                this.SourceDir = "src";
            else
                this.SourceDir = this.SourceDir.Replace('\\', '/').Trim('/');

            if (string.IsNullOrWhiteSpace(this.DistDir))
                this.DistDir = "dist";
            else
                this.DistDir = this.DistDir.Replace('\\', '/').Trim('/');

            if (this.AllowExts == null || this.AllowExts.Length == 0)
            {
                this.AllowExts = new string[]{
                    ".htm", ".html", ".cshtml", ".config", ".js", ".css", ".json", ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".woff", ".woff2", ".ttf", ".svg", ".eot", ".otf"};
            }

            if (this.DenySuffix == null || this.DenySuffix.Length == 0)
            {
                this.DenySuffix = new string[] { "layout.cshtml", "part.cshtml", "part.js", "part.css" };
            }

            // 不允许发布目录,在此补上全路径
            if (this.DenyDirs != null)
            {
                for (int i = 0; i < this.DenyDirs.Length; i++)
                {
                    string tmp = this.DenyDirs[i].Replace('\\', '/').Trim('/');
                    this.DenyDirs[i] = $"{EnvVar.ProjectDir}/{tmp}";
                }
            }

            // 不允许发布文件,在此补上全路径
            if (this.DenyFiles != null)
            {
                for (int i = 0; i < this.DenyFiles.Length; i++)
                {
                    string tmp = this.DenyFiles[i].Replace('\\', '/').Trim('/');
                    this.DenyFiles[i] = $"{EnvVar.ProjectDir}/{tmp}";
                }
            }
        }
        /// <summary>
        /// 生成默认json配置文件.以json格式文本形式返回.
        /// 在this.CheckVal()执行后再执行此文件
        /// 此方法只在项目还没有配置文件时执行一次.
        /// </summary>
        public string CreateJson()
        {
            string k = "";
            string v = "";
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendLine("{");

            //
            sb.AppendLine("  // 源文件目录名(例如:src 相对路径,相对于项目根目录)");
            k = nameof(this.SourceDir);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = this.SourceDir;
            sb.AppendLine($"  \"{k}\": \"{v}\",");

            //
            sb.AppendLine("  // 发布目录名(例如: 'dist' 或 'd:/pubdir' 相对路径或绝对路径)");
            k = nameof(this.DistDir);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = this.DistDir;
            sb.AppendLine($"  \"{k}\": \"{v}\",");

            //
            sb.AppendLine("  // 发布整个项目时,发布前删除发布目录所有内容");
            k = nameof(this.EmptyPublishDir);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = this.EmptyPublishDir.ToString().ToLower();
            sb.AppendLine($"  \"{k}\": \"{v}\",");

            //
            sb.AppendLine("  // 发布整个项目时,编译项目后发布bin文件夹");
            k = nameof(this.BuildBin);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = this.BuildBin.ToString().ToLower();
            sb.AppendLine($"  \"{k}\": \"{v}\",");

            //
            sb.AppendLine("  // 对js,css,html压缩输出(0=不压缩,7=都压缩. 1=html,2=css,4=js)");
            k = nameof(this.MiniOutput);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = this.MiniOutput.ToString();
            sb.AppendLine($"  \"{k}\": {v},");

            //
            sb.AppendLine("  // 允许发布的文件扩展名");
            k = nameof(this.AllowExts);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = $"[ \"{string.Join("\", \"", this.AllowExts)}\" ]";
            sb.AppendLine($"  \"{k}\": {v},");

            //
            sb.AppendLine("  // 不允许发布的文件后缀名");
            k = nameof(this.DenySuffix);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = $"[ \"{string.Join("\", \"", this.DenySuffix)}\" ]";
            sb.AppendLine($"  \"{k}\": {v},");

            //
            sb.AppendLine("  // 不允许发布的文件(例如:bundleconfig.json data/data.mdb 相对路径的文件名,相对于项目根目录)");
            k = nameof(this.DenyFiles);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = "[]";
            sb.AppendLine($"  \"{k}\": {v},");

            //
            sb.AppendLine("  // 不允许发布的目录(例如:cfg 相对路径,相对于项目根目录)");
            k = nameof(this.DenyDirs);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = "[]";
            sb.AppendLine($"  \"{k}\": {v},");

            //
            sb.AppendLine("  // 公共变量,一个json键值对例如{ name : 'mirror' , ... },将作为匿名对象作为Model传给cshtml文件,调用方法@Model.name");
            k = nameof(this.GlobleVar);
            k = $"{k.Substring(0, 1).ToLower()}{k.Substring(1)}";
            v = "null";
            sb.AppendLine($"  \"{k}\": {v}");

            //
            sb.AppendLine("}");
            //
            return sb.ToString();
        }
    }
}
