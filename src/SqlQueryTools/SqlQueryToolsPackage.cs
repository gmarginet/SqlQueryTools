global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using SqlQueryTools.Options;
using System.Runtime.InteropServices;
using System.Threading;

namespace SqlQueryTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.SqlQueryToolsString)]
    [ProvideOptionPage(typeof(DialogPageProvider.General), "SqlQueryTools", "General", 0, 0, true)]
    [ProvideUIContextRule(PackageGuids.SqlQueryToolsMenuVisibilityConstraintString,
        name: "SqlQueryToolsMenuVisibility",
        expression: "Cs",
        termNames: new[] { "Cs" },
        termValues: new[] { "HierSingleSelectionName:.cs$" })]

    public sealed class SqlQueryToolsPackage : ToolkitPackage
    {
        public OutputWindowPane OutputPane { get; set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            OutputPane = await VS.Windows.CreateOutputWindowPaneAsync("SqlQueryTools");

            await this.RegisterCommandsAsync();
        }
    }
}