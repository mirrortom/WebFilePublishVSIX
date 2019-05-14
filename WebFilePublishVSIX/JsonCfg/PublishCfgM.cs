using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFilePublishVSIX
{
    class PublishCfgM
    {
        /// <summary>
        /// 源文件目录名(例如:src/ 不能以/或者\开头,不能是绝对路径.结尾有/)
        /// </summary>
        public string SourceDir;
        /// <summary>
        /// 发布目录名(例如: 'dist/' 或 'd:/pubdir/' 相对路径或绝对路径,相对路径不以/或\开头,结尾有/)
        /// </summary>
        public string DistDir;
        /// <summary>
        /// 允许发布的文件扩展名列表
        /// </summary>
        public string[] AllowExts;
        /// <summary>
        /// 不允许发布的文件后缀名
        /// </summary>
        public string[] DenyExts;
        /// <summary>
        /// 发布前是否清空发布目录
        /// </summary>
        public bool EmptyPublishDir = true;

        /// <summary>
        /// 将空值或未设定的成员设为默认值
        /// </summary>
        public PublishCfgM InitDef()
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
            if (this.DenyExts == null || this.DenyExts.Length == 0)
            {
                this.DenyExts = new string[] { "layout.cshtml", "part.cshtml", "part.js", "part.css" };
            }
            return this;
        }
    }
}
