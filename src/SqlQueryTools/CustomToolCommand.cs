using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace SqlQueryTools
{
    internal sealed class CustomToolCommand
    {
        private readonly AsyncPackage _package;
        private readonly DTE _dte;

        private CustomToolCommand(AsyncPackage package, OleMenuCommandService commandService, DTE dte)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(PackageGuids.guidSqlQueryToolsPackageCmdSet, PackageIds.CustomToolCommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        public static CustomToolCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CustomToolCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = await package.GetServiceAsync<DTE, DTE>();

            Instance = new CustomToolCommand(package, commandService, dte);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectItem = _dte.SelectedItems.Item(1).ProjectItem;

            projectItem.Properties.Item("CustomTool").Value = SqlStringGenerator.Name;
        }
    }
}
