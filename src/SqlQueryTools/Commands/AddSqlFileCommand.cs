using SqlQueryTools.Dialogs;
using SqlQueryTools.Extensions;
using SqlQueryTools.Options;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlQueryTools
{
    [Command(PackageIds.AddSqlFileCommand)]
    internal sealed class AddSqlFileCommand : BaseCommand<AddSqlFileCommand>
    {
        internal const string GenerateParameterNamesAttributeName = "GenerateParameterNames";
        internal const string GeneratePocoClassAttributeName = "GeneratePocoClass";

        public OutputWindowPane OutputPane => ((SqlQueryToolsPackage)Package).OutputPane;

        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();

            var selectedItemList = (await solutionExplorer.GetSelectionAsync()).ToList();
            if (selectedItemList.Count != 1
                || selectedItemList[0].Type != SolutionItemType.PhysicalFile
                || selectedItemList[0].Name.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                await OutputPane.ActivateAsync();
                await OutputPane.WriteLineAsync("Sql file can not be added, please select a single .cs file.");
                return;
            }
            var selectedPhisicalFile = selectedItemList.First() as PhysicalFile;

            var project = selectedPhisicalFile.ContainingProject;
            if (project == null)
            {
                await OutputPane.ActivateAsync();
                await OutputPane.WriteLineAsync("Sql file can not be added, selected file is not part of a project.");
                return;
            }

            var options = await GeneralOptions.GetLiveInstanceAsync();

            var newFileName = await FileNameDialog.GetFileNameAsync(options.SqlFileSuffix);
            if (string.IsNullOrEmpty(newFileName)
                || newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await OutputPane.ActivateAsync();
                await OutputPane.WriteLineAsync($"Sql file can not be added, '{newFileName}' is an invalid file name.");
                return;
            }

            var newFileDirectory = Path.GetDirectoryName(selectedPhisicalFile.FullPath);
            var newFileFullName = Path.Combine(newFileDirectory, newFileName);
            if (File.Exists(newFileFullName))
            {
                await OutputPane.ActivateAsync();
                await OutputPane.WriteLineAsync($"Sql file can not be added, '{newFileFullName}' already exists.");
                return;
            }

            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine("--Add your declarations here.");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(options.EndOfParameterDeclarationMarker);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("--Add your query here.");

            File.WriteAllText(newFileFullName, contentBuilder.ToString());

            await project.AddNestedFileAsync(newFileFullName, selectedPhisicalFile);

            var newPhisicalFile = project.GetPhysicalFile(newFileFullName);
            if (newPhisicalFile != null)
            {
                await newPhisicalFile.TrySetAttributeAsync(GenerateParameterNamesAttributeName, options.GenerateParameterNames);
                await newPhisicalFile.TrySetAttributeAsync(GeneratePocoClassAttributeName, options.GeneratePocoClass);
                await newPhisicalFile.TrySetAttributeAsync("New Property", "test from code");
                await project.SaveAsync();
            }

            await VS.Documents.OpenAsync(newFileFullName);

            await OutputPane.ActivateAsync();
            await OutputPane.WriteLineAsync($"Successfully added sql file '{newFileFullName}'.");
        }
    }
}
