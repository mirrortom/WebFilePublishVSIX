﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PublishActiveFile
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("36a0806e-5fc9-4548-984d-d6d446146f78");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishActiveFile"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PublishActiveFile(AsyncPackage package, OleMenuCommandService commandService)
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
        public static PublishActiveFile Instance
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
            // Verify the current thread is the UI thread - the call to AddCommand in PublishActiveFile's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new PublishActiveFile(package, commandService);
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
            // 发布当前处于活动状态的1个文件 如果没有活动文件,不动作
            var activedoc = ProjectHelpers.GetActiveDoc();
            if (activedoc == null)
            {
                ErrBox.Info(this.package, "未找到激活的文件"); return;
            }

            // 当前活动项目路径
            Project activeProj = ProjectHelpers.GetActiveProject();
            if (activeProj == null)
            {
                ErrBox.Info(this.package, "未选中WEB项目"); return;
            }
            // 建立发布配置对象
            EnvVar.ProjectDir = activeProj.GetRootFolder();
            string res = PublishHelpers.CreatePublishCfg();
            if (res!=null)
            {
                ErrBox.Info(this.package, res); return;
            }

            // 取得要发布的文件路径
            List<string> srcfiles = new List<string>
            {
                activedoc.FullName
            };
            // 发布处理
            string resinfo=PublishHelpers.PublishFiles(srcfiles);
            if (resinfo != null)
            {
                ErrBox.Info(this.package, resinfo);
            }
        }
    }
}
