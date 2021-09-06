using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SqlQueryTools.Config;
using SqlQueryTools.Extensions;
using SqlQueryTools.Helpers;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace SqlQueryTools
{
    internal sealed class AddSqlFileCommand
    {
        private readonly AsyncPackage _package;
        private readonly DTE2 _dte;

        private AddSqlFileCommand(AsyncPackage package, OleMenuCommandService commandService, DTE2 dte)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(PackageGuids.guidSqlQueryToolsPackageCmdSet, PackageIds.AddSqlFileCommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        public static AddSqlFileCommand Instance { get; private set; }
        private IAsyncServiceProvider ServiceProvider { get { return this._package; } }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CustomToolCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = await package.GetServiceAsync<DTE, DTE2>();

            Instance = new AddSqlFileCommand(package, commandService, dte);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var outputPane = GetSqlQueryToolsOutputPane();

            outputPane.Activate(); // Brings this pane into view

            var selectedItems = (Array) _dte.ToolWindows.SolutionExplorer.SelectedItems;
            if (selectedItems.Length != 1) return;

            var selectedItem = (UIHierarchyItem) selectedItems.Cast<UIHierarchyItem>().First();

            string selectedItemFullPath;
            Project project;
            switch (selectedItem.Object)
            {
                case Project selectedProject:
                {
                    try
                    {
                        selectedItemFullPath = selectedProject.GetProperty<string>("FullPath");
                        project = selectedProject;
                    }
                    catch (ArgumentException)
                    {
                        try
                        {
                            // MFC projects don't have FullPath, and there seems to be no way to query existence
                            selectedItemFullPath = selectedProject.Properties.Item("ProjectDirectory").Value as string;
                            project = selectedProject;
                        }
                        catch (ArgumentException)
                        {
                            // Installer projects have a ProjectPath.
                            selectedItemFullPath = selectedProject.Properties.Item("ProjectPath").Value as string;
                            project = selectedProject;
                        }
                    }
                    break;
                }
                case ProjectItem selectedProjectItem:
                    selectedItemFullPath = selectedProjectItem.GetProperty<string>("FullPath");
                    project = selectedProjectItem.ContainingProject;
                    break;
                default:
                    selectedItemFullPath = string.Empty;
                    project = null;
                    break;
            }
                
            if (string.IsNullOrEmpty(selectedItemFullPath) || project == null) return;

            var dialog = new FileNameDialog();

            var hwnd = new IntPtr(_dte.MainWindow.HWnd);
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;

            var result = dialog.ShowDialog();
            var newFileName = (result.HasValue && result.Value) ? dialog.Input : string.Empty;

            if (string.IsNullOrEmpty(newFileName)) return;

            if (!Path.HasExtension(newFileName)) newFileName += ".sql";

            var newFileDirectory = Path.GetDirectoryName(selectedItemFullPath);
            var newFileFullName = Path.Combine(newFileDirectory, newFileName);

            outputPane.OutputString($"Adding file {newFileFullName}\r\n");

            if (!SqlQueryToolsConfigHelper.GetConfig(project, out var config, out var errors))
            {
                foreach (var error in errors)
                {
                    outputPane.OutputString($"\t{error}\r\n");
                    return;
                }
            }

            if (File.Exists(newFileFullName)) return;

            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine("--Add your declarations here.");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(config.EndOfParameterDeclarationMarker);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("--Add your query here.");

            File.WriteAllText(newFileFullName, contentBuilder.ToString());
            var projectItem = project.ProjectItems.AddFromFile(newFileFullName);
            projectItem.Properties.Item("CustomTool").Value = SqlStringGenerator.Name;
        }

        private IVsOutputWindowPane GetSqlQueryToolsOutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(_package.GetServiceAsync(typeof(SVsOutputWindow)) is IVsOutputWindow outWindow))
                throw new Exception($"Could not get service: {nameof(SVsOutputWindow)}");

            outWindow.GetPane(ref PackageGuids.guidSqlQueryToolsOutputPane, out var outputPane);
            if (outputPane != null) return outputPane;

            outWindow.CreatePane(ref PackageGuids.guidSqlQueryToolsOutputPane, "Sql Query String Tools", 1, 1);
            outWindow.GetPane(ref PackageGuids.guidSqlQueryToolsOutputPane, out outputPane);

            return outputPane;
        }
    }
}
