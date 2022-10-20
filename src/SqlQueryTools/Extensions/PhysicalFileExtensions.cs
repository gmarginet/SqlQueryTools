using System.Threading.Tasks;

namespace SqlQueryTools.Extensions
{
    public static class PhysicalFileExtensions
    {
        internal const string GenerateParameterNamesAttributeName = "SqlQueryToolsGenerateParameterNames";
        internal const string GeneratePocoClassAttributeName = "SqlQueryToolsGeneratePocoClass";

        public static async Task<bool> GetGenerateParameterNamesAsync(this PhysicalFile physicalFile, bool defaultValue = false)
        {
            var value = await physicalFile.GetAttributeAsync(GenerateParameterNamesAttributeName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return bool.Parse(value);
            }

            await physicalFile.TrySetGenerateParameterNamesAsync(defaultValue);
            return defaultValue;
        }

        public static async Task<bool> TrySetGenerateParameterNamesAsync(this PhysicalFile physicalFile, bool value)
        {
            return await physicalFile.TrySetAttributeAsync(GenerateParameterNamesAttributeName, value);
        }

        public static async Task<bool> GetGeneratePocoClassAsync(this PhysicalFile physicalFile, bool defaultValue = false)
        {
            var value = await physicalFile.GetAttributeAsync(GeneratePocoClassAttributeName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return bool.Parse(value);
            }

            await physicalFile.TrySetGeneratePocoClassAsync(defaultValue);
            return defaultValue;
        }

        public static async Task<bool> TrySetGeneratePocoClassAsync(this PhysicalFile physicalFile, bool value)
        {
            return await physicalFile.TrySetAttributeAsync(GeneratePocoClassAttributeName, value);
        }
    }
}
