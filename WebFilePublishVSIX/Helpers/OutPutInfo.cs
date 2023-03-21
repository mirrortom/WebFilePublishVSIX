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
        /// 信息提示弹出框
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(string msg)
        {
            VS.MessageBox.ShowWarning(msg, EnvVar.Name);
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
        public static async Task VsOutPutWindow(string msg, bool clear = false)
        {
            OutputWindowPane panel;
            if (EnvVar.outPutWinPanelGuid != default)
            {
                panel = await VS.Windows.GetOutputWindowPaneAsync(EnvVar.outPutWinPanelGuid);
            }
            else
            {
                panel = await VS.Windows.CreateOutputWindowPaneAsync(EnvVar.Name);
                EnvVar.outPutWinPanelGuid = panel.Guid;
            }


            // 清空消息
            if (clear)
                await panel.ClearAsync();

            // 激活输出窗口的该面板
            await panel.ActivateAsync();

            // 输出消息
            await panel.WriteLineAsync(msg);

            // 显示(激活) vs"输出"窗口
            var window = await VS.Windows.FindWindowAsync(EnvVar.outPutWindowGuid);
            await window.ShowAsync();
        }
    }
}
