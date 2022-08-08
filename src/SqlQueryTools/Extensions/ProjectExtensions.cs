using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlQueryTools.Extensions
{
    public static class ProjectExtensions
    {
        public const string SqlQueryToolsConnectionStringAttributeName = "SqlQueryToolsConnectionString";

        public static async Task<string> GetSqlQueryToolsConnectionStringAsync(this Project project)
        {
            var connectionString = await project.GetAttributeAsync(SqlQueryToolsConnectionStringAttributeName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = string.Empty;
                await project.TrySetAttributeAsync(SqlQueryToolsConnectionStringAttributeName, connectionString);
                await project.SaveAsync();
            }

            return connectionString;
        }

        public static async Task AddNestedFileAsync(this Project project, string newFilePath, PhysicalFile dependentUponFile)
        {
            var newPhysicalFile = project.GetPhysicalFile(newFilePath);
            if (newPhysicalFile == null)
            {
                await project.AddExistingFilesAsync(newFilePath);
                newPhysicalFile = project.GetPhysicalFile(newFilePath);
                await newPhysicalFile.TrySetAttributeAsync(PhysicalFileAttribute.DependentUpon, dependentUponFile.Text);
                await project.SaveAsync();
            }
        }

        public static PhysicalFile GetPhysicalFile(this Project project, string filePath)
        {
            return GetPhysicalFileRecursive(project, filePath);
        }

        private static PhysicalFile GetPhysicalFileRecursive(SolutionItem parent, string filePath)
        {
            foreach (var item in parent.Children)
            {
                if (item != null && item is PhysicalFile && item.FullPath == filePath)
                {
                    return (PhysicalFile)item;
                }

                var result = GetPhysicalFileRecursive(item, filePath);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
