using Dapper;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SqlQueryTools.Extensions;
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

            var connectionString = await project.GetSqlQueryToolsConnectionStringAsync();
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to connection string not set!");
                await outputPane.WriteLineAsync($"\t\tPlease set the connection string in property '{ProjectExtensions.SqlQueryToolsConnectionStringAttributeName}' in project file '{project.FullPath}'.");
                return;
            }

            var sqlFileContent = doc.TextBuffer.CurrentSnapshot.GetText();
            var parser = new TSql160Parser(false);
            var sqlParserTokens = parser.GetTokenStream(new StringReader(sqlFileContent), out var sqlParserErrors);
            if (sqlParserErrors.Count > 0)
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to the following sql parse errors:");
                foreach (var error in sqlParserErrors)
                {
                    await outputPane.WriteLineAsync($"\t\t{error.Message} (line={error.Line}, column={error.Column})");
                }
                return;
            }

            var hasDeclarationSection = sqlParserTokens.Any(x => x.TokenType == TSqlTokenType.SingleLineComment && x.Text == options.EndOfParameterDeclarationMarker);
            var endOfParameterDeclarationMarkerFound = false;
            var declaredParameters = new Dictionary<string,string>();
            var foundParameterDeclaration = String.Empty;
            var sqlCodeBuilder = new StringBuilder();
            var whiteSpaceBuffer = new StringBuilder();
            foreach (var token in sqlParserTokens)
            {
                if (hasDeclarationSection && endOfParameterDeclarationMarkerFound == false)
                {
                    if (token.TokenType == TSqlTokenType.SingleLineComment && token.Text == options.EndOfParameterDeclarationMarker)
                    {
                        endOfParameterDeclarationMarkerFound = true;
                        continue;
                    }

                    if (token.TokenType == TSqlTokenType.Variable)
                    {
                        foundParameterDeclaration = token.Text;
                        declaredParameters.Add(foundParameterDeclaration, string.Empty);
                        continue;
                    }

                    if (token.TokenType == TSqlTokenType.Identifier && !string.IsNullOrEmpty(foundParameterDeclaration))
                    {
                        declaredParameters[foundParameterDeclaration] = token.Text;
                        foundParameterDeclaration = string.Empty;
                    }
                }
                else
                {
                    if (token.TokenType == TSqlTokenType.SingleLineComment)
                    {
                        continue;
                    }
                    
                    if (token.TokenType == TSqlTokenType.MultilineComment)
                    {
                        continue;
                    }
                    
                    if (token.TokenType == TSqlTokenType.WhiteSpace)
                    {
                        if (sqlCodeBuilder.Length > 0)
                        {
                            whiteSpaceBuffer.Append(token.Text);
                        }
                        continue;
                    }

                    if (token.TokenType == TSqlTokenType.EndOfFile)
                    {
                        continue;
                    }

                    if (token.TokenType == TSqlTokenType.Variable)
                    {
                        if (!declaredParameters.ContainsKey(token.Text))
                        {
                            await outputPane.WriteLineAsync($"\tVariable {token.Text} is used in the query, but it is not defined in the 'Parameter Declarat");
                            return;
                        }
                    }

                    if (whiteSpaceBuffer.Length > 0)
                    {
                        sqlCodeBuilder.Append(whiteSpaceBuffer.ToString());
                        whiteSpaceBuffer.Clear();
                    }

                    sqlCodeBuilder.Append(token.Text);
                }
            }

            var sqlCode = sqlCodeBuilder.ToString();
            if (string.IsNullOrWhiteSpace(sqlCode))
            {
                await outputPane.WriteLineAsync("\tCode could not be generated due to no sql code found.");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    try
                    {
                        //conn.Query($"DECLARE @sql NVARCHAR(MAX) = '{sqlCode.Replace("'", "''")}'; EXEC sp_describe_first_result_set @sql;");
                        //conn.Query($"DECLARE @sql NVARCHAR(MAX) = '{sqlCode.Replace("'", "''")}'; DECLARE @params NVARCHAR(MAX) = '{string.Join(", ", declaredParameters.Select(x => $"{x.Key} {x.Value}"))}';  EXEC sp_describe_first_result_set @sql, @params;").ToList();
                        var queryBuilder = new StringBuilder();
                        queryBuilder.AppendLine($"DECLARE @sql NVARCHAR(MAX) = '{sqlCode.Replace("'", "''")}';");
                        queryBuilder.AppendLine($"DECLARE @params NVARCHAR(MAX) = '{string.Join(", ", declaredParameters.Select(x => $"{x.Key} {x.Value}"))}';");
                        queryBuilder.AppendLine("EXEC sp_describe_first_result_set @sql, @params;");
                        conn.Query(queryBuilder.ToString()).ToList();
                    }
                    catch (SqlException ex)
                    {
                        await outputPane.WriteLineAsync("\tCode could not be generated due to the following sql parse errors:");

                        foreach (SqlError sqlError in ex.Errors)
                        {
                            if (sqlError.Number == 11529 || sqlError.Number == 11501) continue;

                            await outputPane.WriteLineAsync($"\t\t{sqlError.Message}");
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
            contentBuilder.AppendLine($"\t\tpublic const string Query = @\"{sqlCode}\";");

            foreach (var name in declaredParameters.Keys)
            {
                contentBuilder.AppendLine();
                contentBuilder.AppendLine($"\t\tpublic const string {name} = @\"{name}\";");
            }

            contentBuilder.AppendLine($"\t}}");
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
