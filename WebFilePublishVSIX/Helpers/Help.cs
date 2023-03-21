namespace WebFilePublishVSIX;

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
}