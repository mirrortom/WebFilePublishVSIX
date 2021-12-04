using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// Command 发布选中目录
    /// </summary>
    internal sealed class PublishDir
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 5003;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7b3b3c16-61a5-44c5-9d5e-4f8c679acfa4");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishDir"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PublishDir(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PublishDir Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in PublishDir's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PublishDir(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            // 右键点击后选中的文件夹(一般是一个,但也可以多个)
            var selectedDirs = ProjectHelpers.GetSelectedItemPaths();
            if (selectedDirs.Count() == 0)
            {
                OutPutInfo.Info("未选中文件夹!"); return;
            }

            // 当前活动项目路径
            Project activeProj = ProjectHelpers.GetActiveProject();
            if (activeProj == null)
            {
                OutPutInfo.Info("未选中WEB项目"); return;
            }
            // 建立发布配置对象
            EnvVar.ProjectDir = activeProj.GetRootFolder();
            string res = PublishHelpers.CreatePublishCfg();
            if (res != null)
            {
                OutPutInfo.Info(res); return;
            }

            // 取得要发布的文件路径 
            List<string> srcfiles = new List<string>();
            foreach (string dirPath in selectedDirs)
            {
                // 选中的可能是文件和目录,如果是目录才取出其中文件和子目录文件
                if (Directory.Exists(dirPath))
                    srcfiles.AddRange(Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories));
            }
            if (srcfiles.Count == 0)
            {
                OutPutInfo.Info("至少选择一个文件夹!");
                return;
            }
            // 发布处理
            // 发布处理
            Task.Factory.StartNew(() =>
            {
                string resinfo = PublishHelpers.PublishFiles(srcfiles);
                if (resinfo != null)
                {
                    OutPutInfo.Info(resinfo);
                }
            });
        }
    }
}
