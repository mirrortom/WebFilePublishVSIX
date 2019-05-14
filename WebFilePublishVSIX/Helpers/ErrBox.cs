using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebFilePublishVSIX
{
    class ErrBox
    {
        public static void Info(string msg, string title = "插件-信息提示")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void Error(Exception ex, string title = "插件-异常提示")
        {
            MessageBox.Show(ex.ToString() + '\n' + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void Info(IServiceProvider server, string msg,string title = "插件-信息提示")
        {
            VsShellUtilities.ShowMessageBox(
               server,
               msg,
               title,
               OLEMSGICON.OLEMSGICON_INFO,
               OLEMSGBUTTON.OLEMSGBUTTON_OK,
               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        public static void Error(IServiceProvider server, Exception ex, string title = "插件-异常提示")
        {
            string msg = ex.ToString() + '\n' + ex.Message;
            VsShellUtilities.ShowMessageBox(
               server,
               msg,
               title,
               OLEMSGICON.OLEMSGICON_WARNING,
               OLEMSGBUTTON.OLEMSGBUTTON_OK,
               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
