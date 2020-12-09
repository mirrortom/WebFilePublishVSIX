using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// 发布帮助类
    /// </summary>
    static class PublishHelpers
    {
        /// <summary>
        /// 用于获取VS相关信息.如VS的"输出"窗口
        /// </summary>
        private static DTE2 _dte = PublishFilePackage._dte;

        /// <summary>
        /// 发布配置文件对象(用于公用)
        /// </summary>
        internal static PublishCfgM JsonCfg { get; private set; }

        #region 获取json配置文件
        /// <summary>
        /// 根据发布配置文件生成发布配置对象.配置文件publish.json放在项目根目录下.
        /// 如果没有配置文件,将使用默认配置,并在项目根目录下生成publish.json配置文件
        /// 如果指定的配置文件不是有效的json,返回出错提示.
        /// 成功返回null
        /// </summary>
        /// <returns></returns>
        internal static string CreatePublishCfg()
        {
            try
            {
                string jsonPath = $"{EnvVar.ProjectDir}/{EnvVar.PublishCfgName}";
                if (!File.Exists(jsonPath))
                {
                    // 没有配置文件时,在项目根目录下生成publish.json配置文件
                    JsonCfg = new PublishCfgM();
                    JsonCfg.CheckVal();
                    using (StreamWriter sw = new StreamWriter(jsonPath))
                    {
                        sw.Write(JsonCfg.CreateJson());
                    }
                    return null;
                }
                // 已有时
                JsonCfg = Newtonsoft.Json.JsonConvert.DeserializeObject<PublishCfgM>(File.ReadAllText(jsonPath));
                JsonCfg.CheckVal();
                return null;
            }
            catch (Exception e)
            {
                return $"publish.json配置文件生成失败!异常信息:{Environment.NewLine}{e.ToString()}";
            }
        }
        #endregion

        #region 目录或路径计算

        /// <summary>
        /// 建立输出目录,如果不存在时.
        /// 成功返回null,失败返回出错信息("error"打头)
        /// </summary>
        /// <returns></returns>
        private static string CreateOutDir()
        {
            if (!Path.IsPathRooted(JsonCfg.DistDir))
                JsonCfg.DistDir = $"{EnvVar.ProjectDir}/{JsonCfg.DistDir}";
            try
            {
                Directory.CreateDirectory(JsonCfg.DistDir);
                return null;
            }
            catch (Exception e)
            {
                return $"error:发布目录建立失败!异常信息:{Environment.NewLine}{e.ToString()}";
            }
        }
        /// <summary>
        /// 获取源代码目录路径
        /// </summary>
        /// <returns></returns>
        //private static string SrcDir()
        //{
        //    // 文件从项目起始的相对路径,从src目录起,不含src.
        //    return Path.Combine(EnvVar.ProjectDir, JsonCfg.SourceDir);
        //}
        /// <summary>
        /// 在发布前删除发布目录内的所有文件.
        /// 此方法在只在发布整个项目时会用到
        /// </summary>
        internal static string EmptyPuslishDir()
        {
            if (JsonCfg.EmptyPublishDir == true)
            {
                return FileHelpers.EmptyDir(JsonCfg.DistDir);
            }
            return null;
        }
        /// <summary>
        /// 根据要发布的源文件路径,计算出目标路径.如果路径上不存在目录,则生成之
        /// </summary>
        /// <param name="sPath">源文件路径</param>
        /// <param name="targetDir">发布目录路径</param>
        /// <returns></returns>
        private static string TargetPath(string sPath, string targetDir)
        {
            // 源文件从项目根目录起始的相对路径
            string relPath = sPath.Substring(EnvVar.ProjectDir.Length + 1);

            // 如果路径以 "JsonCfg.SourceDir"的值 开头,那么去掉这一级.发布目录不要源代码根目录这一级.
            if (relPath.StartsWith(JsonCfg.SourceDir))
                relPath = relPath.Substring(JsonCfg.SourceDir.Length + 1);

            // 目标路径
            string targetPath = $"{targetDir}/{relPath}";

            // 目录不存在则生成
            //try
            //{
            //}
            //catch (Exception e)
            //{
            //return $"error:目标路径生成失败,异常信息:{e.Message}";
            //}
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            return targetPath;
        }
        #endregion

        #region 发布文件

        /// <summary>
        /// 将文件发布到目录,根据指定的配置.
        /// 成功返回null.失败返回出错信息
        /// </summary>
        /// <param name="filesPath"></param>
        internal static string PublishFiles(List<string> filesPath)
        {
            // 建立发布目录
            string outDir = CreateOutDir();
            if (outDir != null)
                return outDir;

            // 使用预定规则,选出符合发布要求的文件
            List<string> files = FilterFiles(filesPath);
            if (files.Count == 0)
            {
                return "没有找到符合发布要求的文件";
            }

            // vs输出窗口日志信息
            StringBuilder info = new StringBuilder();
            info.AppendLine($"<开始发布文件:>>>>>>>>>>>>>>>>>>>>>>>>>");

            // 按预定规则发布文件
            for (int i = 0; i < files.Count; i++)
            {
                string itemPath = files[i];

                // 计算并生成目标路径目录
                string targetPath = TargetPath(itemPath, JsonCfg.DistDir);

                // 发布文件
                string resFile = OutPutFile(itemPath, targetPath);
                info.AppendLine($"{i + 1}. {targetPath} {resFile}");
            }
            info.AppendLine($"<<<<<<<<<<<<<<<<<发布结束.总共[ {files.Count} ]个>");
            OutPutMsg(info.ToString());
            return null;
        }
        /// <summary>
        /// 输出文件,如果是js,css,则判断是否需要压缩输出,cshtml文件则编译,其它文件原样输出
        /// </summary>
        /// <param name="sPath">原路径</param>
        /// <param name="tPath">目标路径</param>
        /// <returns></returns>
        private static string OutPutFile(string sPath, string tPath)
        {
            string extName = Path.GetExtension(sPath).ToLower();
            //
            if (extName == ".cshtml")
            {
                return OutPutCshtml(sPath, tPath);
            }

            string msg = "发布成功";
            // js
            if (extName == ".js" && new int[] { 4, 5, 6, 7 }.Contains(JsonCfg.MiniOutput))
            {
                string js = Minifier.Js(File.ReadAllText(sPath));
                if (js == null)
                {
                    File.Copy(sPath, tPath, true);
                    return "发布成功,但压缩js失败,请检查js语法错误";
                }
                File.WriteAllText(tPath, js);
                return msg;
            }
            // css
            if (extName == ".css" && new int[] { 2, 3, 6, 7 }.Contains(JsonCfg.MiniOutput))
            {
                string css = Minifier.Css(File.ReadAllText(sPath));
                if (css == null)
                {
                    File.Copy(sPath, tPath, true);
                    return "发布成功,但压缩js失败,请检查js语法错误";
                }
                File.WriteAllText(tPath, css);
                return msg;
            }
            // html
            if (extName.IndexOf(".html") >= 0 && new int[] { 1, 3, 5, 7 }.Contains(JsonCfg.MiniOutput))
            {
                string html = Minifier.Html(File.ReadAllText(sPath));
                // 如果压缩不成功,直接输出原html
                if (html == null)
                {
                    File.Copy(sPath, tPath, true);
                    return "发布成功,但压缩html失败,请检查html语法错误";
                }
                File.WriteAllText(tPath, html);
                return msg;
            }
            // 其它文件,直接输出
            File.Copy(sPath, tPath, true);
            return msg;
        }
        /// <summary>
        /// 编译后输出cshtml文件,返回出错信息
        /// </summary>
        /// <param name="sPath">原路径</param>
        /// <param name="tPath">目标路径</param>
        /// <returns></returns>
        private static string OutPutCshtml(string sPath, string tPath)
        {
            Tuple<bool, string> result = JsonCfg.GlobleVar == null ?
                RazorCshtml.BuildCshtml(sPath) :
                RazorCshtml.BuildCshtml(sPath, JsonCfg.GlobleVar);
            if (result.Item1 == false)
            {
                // 编译失败时
                return $"发布失败{Environment.NewLine}{result.Item2}";
            }

            // 改成html扩展名
            string targetPath = $"{tPath.Substring(0, tPath.Length - "CSHTML".Length)}html";

            // 选择压缩输出时
            if (new int[] { 1, 3, 5, 7 }.Contains(JsonCfg.MiniOutput))
            {
                string html = Minifier.Html(result.Item2);
                // 如果压缩不成功,直接输出原html
                if (html == null)
                {
                    File.WriteAllText(targetPath, result.Item2);
                    return "发布成功,但压缩html失败,请检查html语法错误";
                }
                File.WriteAllText(targetPath, html);
                return "发布成功";
            }
            File.WriteAllText(targetPath, result.Item2);
            return "发布成功";
        }
        /// <summary>
        /// 使用预定规则将不需要发布处理的文件清除,返回新文件列表.
        /// </summary>
        /// <param name="filesPath"></param>
        /// <returns></returns>
        private static List<string> FilterFiles(List<string> filesPath)
        {
            List<string> files = new List<string>();
            string outDirLower = JsonCfg.DistDir.ToLower();
            string jsonCfgPathLower = $"{EnvVar.ProjectDir}/{EnvVar.PublishCfgName}".ToLower();
            // 循环所有文件,筛选
            foreach (var itemPath in filesPath)
            {
                string filePath = itemPath.Replace('\\', '/');
                string filePathLower = filePath.ToLower();

                // 不发布配置文件
                if (filePathLower == jsonCfgPathLower)
                    continue;

                // 可能是个目录,要排除
                if (Directory.Exists(filePath))
                    continue;

                // 只发布支持的扩展名
                if (JsonCfg.AllowExts.FirstOrDefault(o => o.ToLower() == Path.GetExtension(filePathLower)) == null)
                {
                    continue;
                }

                // 排除不允许发布的文件后缀名
                if (JsonCfg.DenySuffix.FirstOrDefault
                    (o => filePathLower.EndsWith(o.ToLower())) != null)
                {
                    continue;
                }

                // 排除不允许发布的目录,比较文件目录,是否为禁发目录开头
                if (JsonCfg.DenyDirs != null)
                {
                    if (JsonCfg.DenyDirs.FirstOrDefault
                        (o => filePathLower.StartsWith(o.ToLower())) != null)
                        continue;
                }

                // 排除不允许发布的文件,比较文件全路径名
                if (JsonCfg.DenyFiles != null)
                {
                    if (JsonCfg.DenyFiles.FirstOrDefault
                        (o => filePathLower == o.ToLower()) != null)
                        continue;
                }

                // 如果文件位于发布目录下要排除掉.例如发布目录是默认值dist时,
                // 是位于项目根目录下的dist文件夹,如果包含进项目,就会发生此情况
                if (filePathLower.StartsWith(outDirLower))
                    continue;
                //
                files.Add(filePath);
            }
            return files;
        }

        /// <summary>
        /// 发布BIN目录.(在编译项目之后调用)
        /// sourcebinDir参数为项目根目录起始的相对目录路径 如 bin/debug(正斜杠,后面无/)
        /// </summary>
        /// <param name="sourcebinDir"></param>
        internal static string PublishBin(string sourcebinDir)
        {
            try
            {
                // 源bin目录全路径
                string fromBinDir = $"{EnvVar.ProjectDir}/{sourcebinDir}";

                // 目标bin目录
                string targetDir = $"{JsonCfg.DistDir}/bin/";

                // 源bin目录下所有文件
                string[] binFiles = Directory.GetFiles(fromBinDir, "*", SearchOption.AllDirectories);

                // vs输出窗口显示信息
                StringBuilder info = new StringBuilder();
                info.AppendLine($"<开始发布bin目录:>>>>>>>>>>>>>>>>>>>>>>>>>");
                info.AppendLine($"从: {fromBinDir} 到: {targetDir}");

                for (int i = 0; i < binFiles.Length; i++)
                {
                    string itemPath = binFiles[i];
                    string targetPath = itemPath.Replace(fromBinDir, targetDir);
                    // 目标路径目录如果不存在,则建立之.
                    string targetFileDir = Path.GetDirectoryName(targetPath);
                    Directory.CreateDirectory(targetFileDir);
                    //
                    File.Copy(itemPath, targetPath, true);
                    info.AppendLine($"{i + 1} 已发布 {targetPath}");
                }
                info.AppendLine($"<<<<<<<<<<<<<<<bin目录文件发布结束.总共[ {binFiles.Length} ]个>");
                //
                OutPutMsg(info.ToString());
                return null;
            }
            catch (Exception e)
            {
                return $"error:bin目录发布时发生异常:{Environment.NewLine}{e.ToString()}{Environment.NewLine}";
            }
        }

        #endregion

        #region 将操作信息输出到VS的"输出"窗口
        /// <summary>
        /// 输出信息到VS的"输出"窗口,如果"输出"窗口未打开,则打开后再输出
        /// </summary>
        /// <param name="msg"></param>
        internal static void OutPutMsg(string msg, bool clear = false)
        {

            EnvDTE.OutputWindowPanes panels =
                _dte.ToolWindows.OutputWindow.OutputWindowPanes;

            // 输出窗口中的一个自定义项的标题
            string title = "发布插件 消息";
            // 
            EnvDTE.OutputWindowPane panel = null;
            foreach (EnvDTE.OutputWindowPane item in panels)
            {
                if (item.Name == title)
                {
                    panel = item;
                    break;
                }
            }
            if (panel == null)
                panel = panels.Add(title);
            // 清空消息
            if (clear)
                panel.Clear();
            // 激活输出窗口的该面板
            panel.Activate();
            // 输出消息
            panel.OutputString(msg);

            // 显示(激活) vs"输出"窗口
            string winCaption = "输出";
            _dte.Windows.Item(winCaption).Activate();
        }
        #endregion
    }
}
