using EnvDTE;

namespace SqlQueryTools.Extensions
{
    public static class ProjectItemExtensions
    {
        public static T GetProperty<T>(this ProjectItem projectItem, string name)
        {
            return (T) projectItem.Properties.Item(name)?.Value;
        }
    }
}