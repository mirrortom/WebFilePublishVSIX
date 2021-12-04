using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace WebFilePublishVSIX
{
    /// <summary>
    /// Command 发布项目
    /// </summary>
    internal sealed class PublishWeb
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 5004;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a78773fc-5eb9-4758-b643-c54938b4e222");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishWeb"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PublishWeb(AsyncPackage package, OleMenuCommandService commandService)
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
        public static PublishWeb Instance
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
            // Switch to the main thread - the call to AddCommand in PublishWeb's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PublishWeb(package, commandService);
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

            // 发布前删除发布目录下所有文件(根据配置文件的设置而执行)
            string emptyOutDir = PublishHelpers.EmptyPuslishDir();
            if (emptyOutDir != null)
            {
                OutPutInfo.Info(emptyOutDir); return;
            }

            //
            OutPutInfo.VsOutWind("<<<--发布项目-->>>" + Environment.NewLine, true);

            // 如果要发布bin目录,才编译项目
            if (PublishHelpers.JsonCfg.BuildBin == true)
            {
                // 编译项目. buildCfg : debug和release.取自当前选中的编译选项
                string buildCfg = activeProj.ConfigurationManager.ActiveConfiguration.ConfigurationName;
                EnvVar._dte.Solution.SolutionBuild.BuildProject(buildCfg, activeProj.UniqueName, true);
                // 编译后DLL文件所在目录.是一个相对于项目根目录起始的目录
                string outBinDir = activeProj.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString().Replace('\\', '/').Trim('/');

                // 复制dll文件到目标目录
                string resBin = PublishHelpers.PublishBin(outBinDir);
                if (resBin != null)
                {
                    OutPutInfo.Info(resBin); return;
                }
            }

            // 发布项目文件
            // 取得项目中所有文件
            List<string> srcfiles = activeProj.GetItems();
            if (srcfiles.Count == 0)
            {
                OutPutInfo.Info("未发布文件,没有找到适合发布的文件.");
                return;
            }

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
