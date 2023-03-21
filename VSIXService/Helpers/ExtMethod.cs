namespace VSIXService.Helpers;

public static class ExtMethod
{
    /// <summary>
    /// 字符串转为utf8编码的byte[]
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] ToBytesUTF8(this string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
    /// <summary>
    /// 用utf8解码一个byte[]
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringUTF8(this byte[] data)
    {
        return System.Text.Encoding.UTF8.GetString(data);
    }
}
