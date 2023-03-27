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
}