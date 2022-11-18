using System.Threading.Tasks;

namespace SqlQueryTools.Extensions
{
    public static class ProjectExtensions
    {
        public const string SqlQueryToolsConnectionStringAttributeName = "SqlQueryToolsConnectionString";

        public static async Task<string> GetConnectionStringAsync(this Project project, string defaultValue = "")
        {
            var value = await project.GetAttributeAsync(SqlQueryToolsConnectionStringAttributeName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            await project.TrySetConnectionStringAsync(defaultValue);
            return defaultValue;
        }

        public static async Task<bool> TrySetConnectionStringAsync(this Project project, string value)
        {
            return await project.TrySetAttributeAsync(SqlQueryToolsConnectionStringAttributeName, value);
        }

        public static async Task AddNestedFileAsync(this Project project, string newFilePath, PhysicalFile dependentUponFile)
        {
            var newPhysicalFile = project.GetPhysicalFile(newFilePath);
            if (newPhysicalFile == null)
            {
                await project.AddExistingFilesAsync(newFilePath);
                newPhysicalFile = project.GetPhysicalFile(newFilePath);
                await newPhysicalFile.TrySetAttributeAsync(PhysicalFileAttribute.DependentUpon, dependentUponFile.Text);
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
