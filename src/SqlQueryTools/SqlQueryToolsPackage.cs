global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using EnvDTE80;
using Microsoft.VisualStudio;
using SqlQueryTools.Options;
using SqlQueryTools.PropertyExtenders;
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
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]

    public sealed class SqlQueryToolsPackage : ToolkitPackage
    {
        private DTE2 dte;
        private MyExtenderProvider extenderProvider;
        private int extenderProviderCookie;

        public OutputWindowPane OutputPane { get; set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            OutputPane = await VS.Windows.CreateOutputWindowPaneAsync("SqlQueryTools");

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE)) as DTE2;
            extenderProvider = new MyExtenderProvider();

            extenderProviderCookie = dte.ObjectExtenders.RegisterExtenderProvider(VSConstants.CATID.CSharpFileProperties_string,
                "MyExtenderProvider", extenderProvider);

            await this.RegisterCommandsAsync();
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            dte.ObjectExtenders.UnregisterExtenderProvider(extenderProviderCookie);
            extenderProvider = null;
            base.Dispose(disposing);
        }
    }
}