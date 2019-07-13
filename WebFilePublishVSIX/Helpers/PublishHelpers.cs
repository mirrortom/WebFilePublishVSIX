using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// 发布帮助类
    /// </summary>
    class PublishHelpers
    {
        /// <summary>
        /// 用于获取VS相关信息.如VS的"输出"窗口
        /// </summary>
        private static DTE2 _dte = PublishFilePackage._dte;

        /// <summary>
        /// 发布配置文件对象(用于缓存)
        /// </summary>
        internal static PublishCfgM JsonCfg { get; private set; }

        /// <summary>
        /// 获取发布目录路径
        /// </summary>
        /// <returns></returns>
        private static string OutDir()
        {
            // Path.IsPathRooted在" /a 和 c:/a 开头的情况下返回true."
            return Path.IsPathRooted(JsonCfg.DistDir) ? JsonCfg.DistDir : Path.Combine(EnvVar.ProjectDir, JsonCfg.DistDir);
        }
        /// <summary>
        /// 获取源代码目录路径
        /// </summary>
        /// <returns></returns>
        private static string SrcDir()
        {
            // 文件从项目起始的相对路径,从src目录起,不含src.
            return Path.Combine(EnvVar.ProjectDir, JsonCfg.SourceDir);
        }
        #region 获取json配置文件
        /// <summary>
        /// 根据发布配置文件生成发布配置对象.配置文件publish.json放在项目根目录下.
        /// 如果没有配置文件,将使用默认配置.
        /// 如果指定的配置文件不合法,返回出错提示.
        /// 成功返回null
        /// </summary>
        /// <returns></returns>
        internal static string CreatePublishCfg()
        {
            try
            {
                string jsonPath = Path.Combine(EnvVar.ProjectDir, EnvVar.PublishCfgName);
                if (!File.Exists(jsonPath))
                {
                    JsonCfg = new PublishCfgM();
                    JsonCfg.InitDef();
                    return null;
                }
                JsonCfg = Newtonsoft.Json.JsonConvert.DeserializeObject<PublishCfgM>(File.ReadAllText(jsonPath));
                JsonCfg.InitDef();
                return null;
            }
            catch (Exception e)
            {
                return $"发布配置文件生成失败!异常信息:{Environment.NewLine}{e.ToString()}";
            }
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
            string outDir = OutDir();
            try
            {
                Directory.CreateDirectory(outDir);
            }
            catch (Exception e)
            {
                return $"发布目录建立失败!异常信息:{Environment.NewLine}{e.ToString()}";
            }

            // 使用预定规则,清除出不需要发布处理的文件
            List<string> files = FilterFiles(filesPath);
            if (files.Count == 0)
            {
                return "没有找到符合发布要求的文件";
            }

            // vs输出窗口显示信息
            StringBuilder info = new StringBuilder();
            info.AppendLine($"<开始发布>------ 共有 {files.Count} 个文件...");

            // 按预定规则发布文件
            for (int i = 0; i < files.Count; i++)
            {
                string itemPath = files[i];
                // 文件从项目根目录起始的相对路径
                string relPath = itemPath.Substring(EnvVar.ProjectDir.Length).Replace('\\', '/');
                // 如果路径以 JsonCfg.SourceDir 开头,那么去掉这一段.发布目录,不要源代码根目录这一层.
                if (relPath.StartsWith(JsonCfg.SourceDir))
                    relPath = relPath.Substring(JsonCfg.SourceDir.Length);
                // 目标路径
                string targetPath = Path.Combine(outDir, relPath).Replace('\\', '/');

                // 目标路径目录如果不存在,则建立之.
                string targetFileDir = Path.GetDirectoryName(targetPath);
                Directory.CreateDirectory(targetFileDir);

                // 是cshtml文件,则编译后发布
                if (targetPath.EndsWith(".cshtml"))
                {
                    Tuple<bool, string> result = RazorCshtml.BuildCshtml(itemPath);
                    if (result.Item1 == false)
                    {
                        info.AppendLine($"{i + 1} 发布失败 {targetPath} {Environment.NewLine}({result.Item2})");
                        continue;
                    }
                    // 改成html扩展名,并生成文件
                    targetPath = $"{targetPath.Substring(0, targetPath.Length - "CSHTML".Length)}html";
                    File.WriteAllText(targetPath, result.Item2);
                    info.AppendLine($"{i + 1} 已发布 {targetPath}");
                    continue;
                }
                // 是一般文件,原样复制发布
                File.Copy(itemPath, targetPath, true);
                info.AppendLine($"{i + 1} 已发布 {targetPath}");
            }
            info.AppendLine("------<发布结束>");
            OutPutMsg(info.ToString());
            return null;
        }

        /// <summary>
        /// 使用预定规则将不需要发布处理的文件清除,返回新文件列表.
        /// </summary>
        /// <param name="filesPath"></param>
        /// <returns></returns>
        private static List<string> FilterFiles(List<string> filesPath)
        {
            List<string> files = new List<string>();
            string outDirLower = OutDir().Replace('\\', '/').ToLower();
            string jsonPathLower = Path.Combine(EnvVar.ProjectDir, EnvVar.PublishCfgName).Replace('\\', '/').ToLower();
            // 循环所有文件,筛选
            foreach (var itemPath in filesPath)
            {
                string filePath = itemPath.Replace('\\', '/');
                string filePathLower = filePath.ToLower();

                // 不发布配置文件
                if (filePathLower == jsonPathLower)
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
        /// sourcebinDir参数为项目根目录起始的相对目录路径 如 bin/debug/
        /// </summary>
        /// <param name="sourcebinDir"></param>
        internal static string PublishBin(string sourcebinDir)
        {
            try
            {
                // 源bin目录全路径
                string fromBinDir = Path.Combine(EnvVar.ProjectDir, sourcebinDir).Replace('\\', '/');

                // 目标bin目录
                string outDir = OutDir();
                string targetDir = Path.Combine(outDir, "bin/").Replace('\\', '/');

                // 源bin目录下所有文件
                string[] binFiles = Directory.GetFiles(fromBinDir, "*", SearchOption.AllDirectories);

                // vs输出窗口显示信息
                StringBuilder info = new StringBuilder();
                info.AppendLine($"<开始发布bin目录>------ 共有 {binFiles.Length} 个文件...");
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
                info.AppendLine($"------<bin目录发布结束>");
                //
                OutPutMsg(info.ToString());
                return null;
            }
            catch (Exception e)
            {
                return $"bin目录发布时发生异常:{Environment.NewLine}{e.ToString()}{Environment.NewLine}";
            }
        }

        /// <summary>
        /// 在发布前删除发布目录的所有文件.
        /// 此方法在发布整个项目时会用到
        /// </summary>
        internal static string DelPublishDir()
        {
            string outDir = Path.IsPathRooted(JsonCfg.DistDir) ? JsonCfg.DistDir : Path.Combine(EnvVar.ProjectDir, JsonCfg.DistDir);
            return FileHelpers.EmptyDir(outDir);
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
