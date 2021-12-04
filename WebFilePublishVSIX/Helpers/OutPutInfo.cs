using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// 使用winform弹出框和vs输出窗口输出信息
    /// </summary>
    static class OutPutInfo
    {
        /// <summary>
        /// 错误提示弹出框
        /// </summary>
        /// <param name="ex"></param>
        public static void Error(Exception ex)
        {
            MessageBox.Show(ex.Message + '\n' + ex.ToString(),
               EnvVar.Name + "--异常提示",
               MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        /// <summary>
        /// 信息提示弹出框
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(string msg)
        {
            MessageBox.Show(msg,
               EnvVar.Name + "--信息提示",
               MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // 这个弹出框需要server参数,麻烦,用winform方便
        //public static void Info(IServiceProvider server, string msg, string title = "插件-信息提示")
        //{
        //    VsShellUtilities.ShowMessageBox(
        //       server,
        //       msg,
        //       title,
        //       OLEMSGICON.OLEMSGICON_INFO,
        //       OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        //}

        /// <summary>
        /// 在vs的输出窗口的标题为"web发布插件"的窗口里输出文本内容
        /// </summary>
        /// <param name="msg">文本</param>
        /// <param name="clear">是否清空原有信息</param>
        public static void VsOutWind(string msg, bool clear = false)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            // 输出窗口集合
            EnvDTE.OutputWindowPanes panels =
                EnvVar._dte.ToolWindows.OutputWindow.OutputWindowPanes;
            // 输出窗口固定的自定义项标题
            string title = EnvVar.Name;
            try
            {
                // If the pane exists already, write to it.
                panels.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane and write to it.
                panels.Add(title);
            }
            EnvDTE.OutputWindowPane panel = panels.Item(title);
            // 清空消息
            if (clear)
                panel.Clear();
            // 激活输出窗口的该面板
            panel.Activate();
            // 输出消息
            panel.OutputString(msg);

            // 显示(激活) vs"输出"窗口
            string winCaption = "输出";
            EnvVar._dte.Windows.Item(winCaption).Activate();
        }
    }
}
