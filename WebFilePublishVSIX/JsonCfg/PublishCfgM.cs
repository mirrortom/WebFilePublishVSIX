using System.IO;

namespace WebFilePublishVSIX
{
    class PublishCfgM
    {
        /// <summary>
        /// // 源文件目录名(例如:src/ 不能以/或者\开头,是相对路径,相对于项目根目录.结尾有/)
        /// </summary>
        public string SourceDir;
        /// <summary>
        /// 发布目录名(例如: 'dist/' 或 'd:/pubdir/' 相对路径或绝对路径,
        /// 相对路径不以/或\开头,结尾有/)
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
        /// 不允许发布的文件(例如:bundleconfig.json data/data.mdb 
        /// 不能以/或者\开头,是带相对路径的文件名,相对于项目根目录) 
        /// </summary>
        public string[] DenyFiles;

        /// <summary>
        /// 不允许发布的目录(例如:src/ 不能以/或者\开头,是相对路径,相对于项目根目录.结尾有/)
        /// </summary>
        public string[] DenyDirs;

        /// <summary>
        /// 将空值或未设定的成员设为默认值
        /// </summary>
        public void InitDef()
        {
            if (string.IsNullOrWhiteSpace(this.SourceDir))
                this.SourceDir = "src/";

            if (string.IsNullOrWhiteSpace(this.DistDir))
                this.DistDir = "dist/";

            if (this.AllowExts == null || this.AllowExts.Length == 0)
            {
                this.AllowExts = new string[]{
                    ".htm", ".html", ".cshtml", ".config", ".js", ".css", ".json", ".jpg", ".jpeg", ".gif", ".png", ".bmp"};
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
                    this.DenyDirs[i] = Path.Combine(EnvVar.ProjectDir, this.DenyDirs[i]).Replace('\\', '/');
                }
            }

            // 不允许发布文件,在此补上全路径
            if (this.DenyFiles != null)
            {
                for (int i = 0; i < this.DenyFiles.Length; i++)
                {
                    this.DenyFiles[i] = Path.Combine(EnvVar.ProjectDir, this.DenyFiles[i]).Replace('\\', '/');
                }
            }
        }
    }
}
