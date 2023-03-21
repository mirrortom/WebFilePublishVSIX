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
    public static int port = 50_015;
    /// <summary>
    /// 代码片段目录
    /// </summary>
    public static string snippetDir = $"{AppContext.BaseDirectory}codesnippet";
}
