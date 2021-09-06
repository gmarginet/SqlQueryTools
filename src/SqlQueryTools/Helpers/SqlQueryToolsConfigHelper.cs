using System.Collections.Generic;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using SqlQueryTools.Config;
using SqlQueryTools.Extensions;

namespace SqlQueryTools.Helpers
{
    public class SqlQueryToolsConfigHelper
    {
        private const string ConfigFileName = "SqlQueryTools.json";

        public static bool GetConfig(Project project, out SqlQueryToolsConfig config, out List<string> errors)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            errors = new List<string>();
            config = null;

            string configFullFileName;
            if (!project.FindProjectItem(ConfigFileName, out var configProjectItem))
            {
                config = new SqlQueryToolsConfig
                {
                    ConnectionString = "",
                    EndOfParameterDeclarationMarker = "---------- End Of Parameter Declaration ----------",
                    GenerateParameterNames = true
                };
                var projectFolder = Path.GetDirectoryName(project.FullName);
                configFullFileName = Path.Combine(projectFolder, ConfigFileName);
                var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFullFileName, configJson);
                project.ProjectItems.AddFromFile(configFullFileName);
            }
            else
            {
                configFullFileName = configProjectItem.GetProperty<string>("FullPath");
            }

            if (config == null && !string.IsNullOrEmpty(configFullFileName))
            {
                var configJson = File.ReadAllText(configFullFileName);
                config = JsonConvert.DeserializeObject<SqlQueryToolsConfig>(configJson);
            }

            if (config == null)
            {
                errors.Add($"Could not read/create {configFullFileName}");
                return false;
            }

            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                errors.Add("ConnectionString property is not set in {configFullFileName}");
            }

            if (string.IsNullOrEmpty(config.EndOfParameterDeclarationMarker))
            {
                errors.Add("EndOfParameterDeclarationMarker property is not set in {configFullFileName}");
            }

            return errors.Count == 0;
        }
    }
}