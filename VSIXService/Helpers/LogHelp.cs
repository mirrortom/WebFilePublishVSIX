﻿using System.Text;

namespace VSIXService.Helpers;

internal class LogHelp
{
    /// <summary>
    /// 日志根目录,默认为当前程序运行目录.要在程序部署前设定.
    /// 默认是程序运行目录.(设定新的必须是绝对路径)(例 e:/logs 或者 /home/log)
    /// </summary>
    private static string LogRootPath = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly ReaderWriterLockSlim LogWriteLock = new();

    /// <summary>
    /// 添加日志 主要针对WIN服务程序,文件位于根目录的SrvLogs目录下
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static void SrvLog(string msg, bool yearDir = false, bool monthDir = false, bool dayDir = false)
    {
        Log(msg, "", "SrvLog", yearDir, monthDir, dayDir);
    }

    /// <summary>
    /// 添加日志  如果不指定目录名,则文件位于根目录的AppLog默认目录下.日志扩展名固定为.log
    /// </summary>
    /// <param name="message">日志内容</param>
    /// <param name="filename">日志文件名,不含扩展名.省略时以当天年月日为名</param>
    /// <param name="directory">一类型日志总目录(应用程序根目录的下一级)</param>
    /// <returns></returns>
    public static void Log(string message, string filename = "", string logDirName = "AppLog", bool yearDir = false, bool monthDir = false, bool dayDir = false)
    {
        try
        {
            // 进入写锁定.如果其它线程也来访问,则等待
            LogWriteLock.EnterWriteLock();

            // 日志目录与文件名
            string directory = GetLogPath(logDirName, yearDir, monthDir, dayDir);
            string fn = filename == "" ? DateTime.Now.Date.ToString("yyyyMMdd") : filename;
            string path = Path.Combine(directory, fn + ".log");
            // 超过2M时存为旧文件,名字如:yyyyMMdd(1)
            CheckFile(path, fn, directory);
            // 开始写入
            Write(path, message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"日志写入异常: {e.Message}");
        }
        finally
        {
            // 解除写锁定
            LogWriteLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    private static void Write(string path, string message)
    {
        using StreamWriter sw = new(path, true, Encoding.UTF8);
        // 日志记录时间.指下面获取的当时时间.不应理解为日志记录的时间.
        // (考虑到并发时,日志缓存到了队列,或者本方法正被访问,
        //   其它线程正在等读写锁解除
        string WriteTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
        // 当前调用该日志方法的线程ID
        //string ThreadId = Environment.CurrentManagedThreadId.ToString();
        // 方法名
        //string method = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName;
        //
        sw.WriteLine($"date:{WriteTime}{Environment.NewLine}{message}{Environment.NewLine}");
    }

    /// <summary>
    /// 检查日志文件 超过2M,保存后,新建.
    /// </summary>
    /// <param name="path">日志全名</param>
    /// <param name="fn">日志名</param>
    /// <param name="directory">日志目录</param>
    private static void CheckFile(string path, string fn, string directory)
    {
        if (!File.Exists(path))
            return;
        FileInfo fi = new(path);
        if (fi.Length < 2000 * 1000)
            return;
        int count = 1;
        while (true)
        {
            string oldpath = Path.Combine(directory, $"{fn}({count}).txt");
            if (!File.Exists(oldpath))
            {
                fi.MoveTo(oldpath);
                break;
            }
            count++;
        }
    }

    /// <summary>
    /// 获取日志根目录 如 e:/logs/ 如果目录不存在,则会建立
    /// 注意:未加异常判断.请保证根目录设置(可能在webconfig的rootPath)及目录名有效
    /// </summary>
    /// <param name="logDirName">日志根目录的名字</param>
    /// <param name="day">是否以天建立目录</param>
    /// <param name="month">是否以月建立目录</param>
    /// <param name="year">是否以年建立目录</param>
    /// <returns></returns>
    private static string GetLogPath(string logDirName = "", bool yearDir = false, bool monthDir = false, bool dayDir = false)
    {
        string logPath = logDirName == "" ? "Logs" : logDirName;
        string rootpath = string.IsNullOrWhiteSpace(LogRootPath) ? AppDomain.CurrentDomain.BaseDirectory : LogRootPath;
        string directory = string.Format(@"{0}/{1}/", rootpath, logPath);
        if (yearDir == true)
            directory = string.Format(@"{0}{1}y/", directory, DateTime.Today.Year);
        if (monthDir == true)
            directory = string.Format(@"{0}{1}m/", directory, DateTime.Today.Month);
        if (dayDir == true)
            directory = string.Format(@"{0}{1}d/", directory, DateTime.Today.Day);
        //
        Directory.CreateDirectory(directory);
        //
        return directory;
    }
}