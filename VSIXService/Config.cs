﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
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
#if DEBUG
    public const int port = 50_016;
#else
    public const int port = 50_015;
#endif
    /// <summary>
    /// 数据接收缓冲大小 16K
    /// </summary>
    public const int size = 16384;
    /// <summary>
    /// 代码片段目录
    /// </summary>
    public static string snippetDir = $"{AppContext.BaseDirectory}codesnippet";
}
