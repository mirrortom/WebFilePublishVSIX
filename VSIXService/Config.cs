using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VSIXService;

internal class Config
{
    public static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
    public const int port = 50_015;
    /// <summary>
    /// 数据接收缓冲大小 4K
    /// </summary>
    public const int size = 4096;
    /// <summary>
    /// 代码片段目录
    /// </summary>
    public static string snippetDir = $"{AppContext.BaseDirectory}codesnippet";
}
