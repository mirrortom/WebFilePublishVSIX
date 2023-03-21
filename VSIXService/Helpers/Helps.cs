using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXService.Helpers;

internal class Helps
{
    /// <summary>
    /// json转化为动态对象
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static dynamic JSONToDynamic(string json)
    {
        return JsonConvert.DeserializeObject(json);
    }

    /// <summary>
    /// querystring转化为动态对象
    /// </summary>
    /// <param name="querystring"></param>
    /// <returns></returns>
    public static dynamic QueryStringToDynamic(string querystring)
    {
        string[] parts = querystring.Split('&');
        dynamic obj = new ExpandoObject();
        foreach (string pair in parts)
        {
            string[] kv = pair.Split('=');
            ((IDictionary<string, object>)obj).Add(kv[0].Trim(), kv[1]);
        }
        return obj;
    }

    /// <summary>
    /// 建立新文件,如果文件存在,则不动作
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    public static void CreateFile(string path, string content = null)
    {
        if (!File.Exists(path))
        {
            // 指定文件编码为UTF8编码格式,否则可能是936中文
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
