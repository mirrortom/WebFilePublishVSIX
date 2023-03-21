﻿using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebFilePublishVSIX;

internal static class FileHelpers
{
    /// <summary>
    /// 获取指定目录中的所有文件
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static List<string> GetAllFiles(string rootDirPath)
    {
        List<string> allfiles = new List<string>();
        void getfiles(string rootDir)
        {
            string[] fs = Directory.GetFiles(rootDir);
            allfiles.AddRange(fs);
            string[] ds = Directory.GetDirectories(rootDir);
            foreach (var dirpath in ds)
            {
                getfiles(dirpath);
            }
        }
        getfiles(rootDirPath);
        return allfiles;
    }

    /// <summary>
    /// 清空文件夹下所有文件和目录.
    /// 清空成功或目录不存在返回null
    /// 清空失败返回出错信息
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string EmptyDir(string dir)
    {
        if (Directory.Exists(dir))
        {
            try
            {
                foreach (string path in Directory.GetFileSystemEntries(dir))
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return $"清空目录时发生异常:{Environment.NewLine}{e.ToString()}";
            }
        }
        return null;
    }
}