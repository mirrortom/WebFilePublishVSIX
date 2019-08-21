using NUglify;

namespace WebFilePublishVSIX
{
    static class Minifier
    {
        /// <summary>
        /// 返回压缩后的html
        /// </summary>
        /// <param name="source">原始html</param>
        /// <returns></returns>
        internal static string Html(string source)
        {
            var result = Uglify.Html(source);
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
