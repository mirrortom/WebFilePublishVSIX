using NUglify;
using NUglify.Html;

namespace WebFilePublishVSIX
{
    static class Minifier
    {
        private static readonly HtmlSettings htmlsetting;
        static Minifier()
        {
            htmlsetting = new HtmlSettings
            {
                // 标签上的属性值为空时不要删除,例如<input value="">
                RemoveEmptyAttributes = false
            };
            // 这3个标记不要优化掉
            // 参考文档:https://github.com/trullock/NUglify/issues/27
            htmlsetting.KeepTags.Add("html");
            htmlsetting.KeepTags.Add("head");
            htmlsetting.KeepTags.Add("body");
        }
        /// <summary>
        /// 返回压缩后的html
        /// </summary>
        /// <param name="source">原始html</param>
        /// <returns></returns>
        internal static string Html(string source)
        {
            var result = Uglify.Html(source, htmlsetting);
            return result.Code;
        }
        /// <summary>
        /// 返回压缩后的css
        /// </summary>
        /// <param name="source">原始css</param>
        /// <returns></returns>
        internal static string Css(string source)
        {
            var result = Uglify.Css(source);
            return result.Code;
        }
        /// <summary>
        /// 返回压缩后的js
        /// </summary>
        /// <param name="source">原始js</param>
        /// <returns></returns>
        internal static string Js(string source)
        {
            var result = Uglify.Js(source);
            return result.Code;
        }
    }
}
