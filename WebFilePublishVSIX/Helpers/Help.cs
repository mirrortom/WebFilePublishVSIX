using System.IO;

namespace WebFilePublishVSIX.Helpers;

internal class Help
{
    /// <summary>
    /// 字符串首字母小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string StartLower(string str)
    {
        char[] a = str.ToCharArray();
        a[0] = char.ToLower(a[0]);
        return new string(a);
    }
    /// <summary>
    /// 去掉路径结尾的斜杠(/或者\)
    /// startEnd=true时,去掉开始和结尾的
    /// </summary>
    /// <returns></returns>
    internal static string PathTrim(string path, bool startEnd = false)
    {
        if (startEnd == true)
        {
            return path.Trim('/', '\\');
        }
        return path.TrimEnd('/', '\\');
    }
    /// <summary>
    /// 将路径的\变为/
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string PathSplitChar(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// 判断path是否属于dirPath下.true:属于.例如: d:/book/index.html属于d:/book
    /// 而d:/bookmark/index.html不属于d:/book
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    internal static bool IsPathInDir(string path, string dirPath)
    {
        if (PathSplitChar(path).StartsWith(PathSplitChar(dirPath)
        , StringComparison.OrdinalIgnoreCase))
        {
            var dirSplitChar = PathTrim(path, true).Substring(PathTrim(dirPath, true).Length, 1);
            return dirSplitChar == "/" || dirSplitChar == "\\";
        }
        return false;
    }

    /// <summary>
    /// 判断两个路径是否相同.
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    internal static bool IsPathEq(string path1, string path2)
    {
        return PathSplitChar(path1).Equals(
        PathSplitChar(path2), StringComparison.OrdinalIgnoreCase);
    }
}