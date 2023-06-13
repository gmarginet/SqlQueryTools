using Dapper;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SqlQueryTools.Dto;
using SqlQueryTools.Extensions;
using SqlQueryTools.Helpers;
using SqlQueryTools.Options;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlQueryTools.FileHandlers
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("SQL Server Tools")]
    [TextViewRole(PredefinedTextViewRoles.Zoomable)]
    internal sealed class SqlSaveHandler : IWpfTextViewCreationListener
    {
        private ITextDocument doc;

        [Import]
        private ITextDocumentFactoryService DocService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (DocService.TryGetTextDocument(textView.TextBuffer, out doc))
            {
                doc.FileActionOccurred += OnSave;
                textView.Closed += TextViewClosed;
            }
        }

        private void OnSave(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await GenerateSqlConstStringAsync(e.FilePath);

                }).FileAndForget(nameof(SqlSaveHandler));
            }
        }

        private async Task GenerateSqlConstStringAsync(string inputFilePath)
        {
            var options = await GeneralOptions.GetLiveInstanceAsync();

            if (!inputFilePath.EndsWith(options.SqlFileSuffix))
            {
                return;
            }

            var outputPane = await VS.Windows.CreateOutputWindowPaneAsync("SqlQueryTools");

            await outputPane.ActivateAsync();
            await outputPane.WriteLineAsync($"Generating code for {inputFilePath}");

            var inputPhysicalFile = await PhysicalFile.FromFileAsync(inputFilePath);
            var project = inputPhysicalFile.ContainingProject;

            var connectionString = await project.GetConnectionStringAsync();
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to connection string not set!");
                await outputPane.WriteLineAsync($"\t\tPlease set the connection string in property '{ProjectExtensions.SqlQueryToolsConnectionStringAttributeName}' in project file '{project.FullPath}'.");
                return;
            }

            var sqlFileContent = doc.TextBuffer.CurrentSnapshot.GetText();
            var parser = new TSql160Parser(false);
            var sqlStatementList = parser.ParseStatementList(new StringReader(sqlFileContent), out var sqlParserStatementsErrors);
            if (sqlParserStatementsErrors.Count > 0)
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to the following sql parse errors:");
                var counter = 0;
                foreach (var error in sqlParserStatementsErrors)
                {
                    counter++;
                    await outputPane.WriteLineAsync($"\t\t{counter}.) {error.Message} (line={error.Line}, column={error.Column})");
                }
                return;
            }

            var parameterDeclarationList = new List<(string Name, string Type)>();
            var sqlCodeBuilder = new StringBuilder();
            foreach (var statement in sqlStatementList.Statements)
            {
                if (statement is DeclareVariableStatement variableStatement)
                {
                    foreach (var declaration in variableStatement.Declarations)
                    {
                        var name = declaration.VariableName.Value;
                        var type = string.Empty;
                        for ( var i = declaration.DataType.FirstTokenIndex; i <= declaration.DataType.LastTokenIndex; i++)
                        {
                            type += declaration.DataType.ScriptTokenStream[i].Text;
                        }
                        parameterDeclarationList.Add((name, type));
                    }
                    continue;
                }

                if (statement is DeclareTableVariableStatement tableVariableStatement)
                {
                    var name = tableVariableStatement.Body.VariableName.Value;
                    parameterDeclarationList.Add((name, null));
                }

                for (int i = statement.FirstTokenIndex; i <= statement.LastTokenIndex; i++)
                {
                    sqlCodeBuilder.Append(statement.ScriptTokenStream[i].Text);
                }

                sqlCodeBuilder.AppendLine();
                sqlCodeBuilder.AppendLine();
            }

            if (sqlCodeBuilder.Length <= 0)
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to no sql code found.");
                return;
            }

            var sqlCode = sqlCodeBuilder.ToString();

            List<SpDescribeFirstResultSetResult> queryMetaData;
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    try
                    {
                        var queryBuilder = new StringBuilder();
                        queryBuilder.AppendLine($"DECLARE @sql NVARCHAR(MAX) = '{sqlCode.Replace("'", "''")}';");
                        queryBuilder.AppendLine($"DECLARE @params NVARCHAR(MAX) = '{string.Join(", ", parameterDeclarationList.Where(x => x.Type != null).Select(x => $"{x.Name} {x.Type}"))}';");
                        queryBuilder.AppendLine("EXEC sp_describe_first_result_set @sql, @params;");
                        queryMetaData = conn.Query<SpDescribeFirstResultSetResult>(queryBuilder.ToString()).ToList();
                    }
                    catch (SqlException ex)
                    {
                        await outputPane.WriteLineAsync("\tCode could not be generated due to the following sql parse errors:");
                        var counter = 0;
                        foreach (SqlError sqlError in ex.Errors)
                        {
                            if (sqlError.Number == 11529 || sqlError.Number == 11501) continue;

                            counter++;
                            await outputPane.WriteLineAsync($"\t\t{counter}.) {sqlError.Message}");
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to the following error:");
                await outputPane.WriteLineAsync($"\t\t{ex.Message}");
                return;
            }

            var inputFileName = Path.GetFileName(inputFilePath);
            var suffixLength = options.SqlFileSuffix.Length;
            var inputFileNameWithoutSuffix = inputFileName.Remove(inputFileName.Length - suffixLength, suffixLength);
            var inputFileDirectory = Path.GetDirectoryName(inputFilePath);

            var className = $"{inputFileNameWithoutSuffix.Replace(".", "")}{options.ClassSuffix}";

            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(className))
            {
                await outputPane.WriteLineAsync($"\tCode could not be generated due to class name '{className}' is not valid.");

                return;
            }

            var fileNamespace = File.ReadAllLines(inputPhysicalFile.Parent.FullPath)
                .Select(l => Regex.Match(l, "^\\s*namespace ([^;]*)"))
                .FirstOrDefault(m => m.Success)
                .Groups[1].Value;

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"//**************************************************************");
            contentBuilder.AppendLine($"// This code was generated by SqlQueryTools ");
            contentBuilder.AppendLine($"//**************************************************************");
            contentBuilder.AppendLine($"// Code is generated on: {DateTime.Now}");
            contentBuilder.AppendLine($"//**************************************************************");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine();
            contentBuilder.AppendLine($"namespace {fileNamespace}");
            contentBuilder.AppendLine($"{{");
            contentBuilder.AppendLine($"\tpublic static class {className}");
            contentBuilder.AppendLine($"\t{{");
            contentBuilder.AppendLine($"\t\tpublic const string {options.SqlStringFieldName} = @\"{sqlCode}\";");

            var generateParameterNames = await inputPhysicalFile.GetGenerateParameterNamesAsync(options.GenerateParameterNames);
            if (generateParameterNames)
            {
                foreach (var name in parameterDeclarationList.Where(x => x.Type != null).Select(x => x.Name))
                {
                    contentBuilder.AppendLine();
                    contentBuilder.AppendLine($"\t\tpublic const string {name} = @\"{name}\";");
                }

            }

            contentBuilder.AppendLine($"\t}}");

            var generatePocoClass = await inputPhysicalFile.GetGeneratePocoClassAsync(options.GeneratePocoClass);
            if (generatePocoClass)
            {
                contentBuilder.AppendLine();
                contentBuilder.AppendLine($"\tpublic class {className}{options.PocoClassSuffix}");
                contentBuilder.AppendLine($"\t{{");

                foreach (var columnInfo in queryMetaData)
                {
                    var columnName = columnInfo.name;
                    var columnType = SqlTypeHelper.ToCSharpTypeName(columnInfo.system_type_name);

                    contentBuilder.AppendLine($"\t\tpublic {columnType} {columnName} {{ get; set; }}");
                }

                contentBuilder.AppendLine($"\t}}");
            }

            contentBuilder.AppendLine($"}}");

            var newFileName = $"{inputFileNameWithoutSuffix}{options.CodeFileSuffix}";
            var newFilePath = Path.Combine(inputFileDirectory, newFileName);

            File.WriteAllText(newFilePath, contentBuilder.ToString());

            await project.AddNestedFileAsync(newFilePath, inputPhysicalFile);

            await outputPane.WriteLineAsync($"\tCode generated successfully!");
        }

        private void TextViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            view.Closed -= TextViewClosed;

            doc.FileActionOccurred -= OnSave;
        }
    }
}
