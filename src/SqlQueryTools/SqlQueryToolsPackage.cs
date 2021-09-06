using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using SqlQueryTools.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SqlQueryTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidSqlQueryToolsPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideCodeGenerator(typeof(SqlStringGenerator), SqlStringGenerator.Name, SqlStringGenerator.Description, true, RegisterCodeBase = true)]
    [ProvideUIContextRule(PackageGuids.guidSqlUIRuleString,
        name: "Sql files",
        expression: "Sql",
        termNames: new[] { "Sql"},
        termValues: new[] { "HierSingleSelectionName:.sql$" })]
    public sealed class SqlQueryToolsPackage : AsyncPackage
    {
        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await CustomToolCommand.InitializeAsync(this);
            await AddSqlFileCommand.InitializeAsync(this);
        }

        #endregion
    }
}
