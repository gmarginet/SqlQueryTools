using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace SqlQueryTools.Extensions
{
    public static class ProjectExtensions
    {
        public static T GetProperty<T>(this Project project, string name)
        {
            return (T) project.Properties.Item(name)?.Value;
        }

        public static bool FindProjectItem(this Project project, string name, out ProjectItem projectItem)
        {
            projectItem = FindProjectItems(project.ProjectItems, (p) => p.Name == name).SingleOrDefault();

            return projectItem != null;
        }

        private static IEnumerable<ProjectItem> FindProjectItems(ProjectItems projectItems, Func<ProjectItem,bool> criteria)
        {
            foreach (ProjectItem projectItem in projectItems)
            {
                if (criteria(projectItem))
                    yield return projectItem;

                if (projectItem.ProjectItems != null)
                {
                    foreach (var subItem in FindProjectItems(projectItem.ProjectItems, criteria))
                        yield return subItem;
                }
                if (projectItem.SubProject != null)
                {
                    foreach (var subItem in FindProjectItems(projectItem.SubProject.ProjectItems, criteria))
                        yield return subItem;
                }
            }
        }
    }
}