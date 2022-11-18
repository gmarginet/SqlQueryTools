using EnvDTE;
using System.Linq;
using Project = Community.VisualStudio.Toolkit.Project;

namespace SqlQueryTools.ObjectExtenders
{
    public class ProjectFileObjectExtenderProvider : IExtenderProvider
    {
        public object GetExtender(string extenderCatid, string extenderName,
             object extendeeObject, IExtenderSite extenderSite, int cookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var project = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
                var selectedItems = await solutionExplorer.GetSelectionAsync();
                var selectedItemList = selectedItems.ToList();

                return selectedItemList.Count == 1
                    && selectedItemList[0].Type == SolutionItemType.Project
                    ? selectedItemList[0] as Project
                    : null;
            });

            return CanExtend(extenderCatid, extenderName, extendeeObject)
                && project != null
                ? new ProjectFileObjectExtender(project, extenderSite, cookie)
                : null;
        }

        public bool CanExtend(string extenderCatid, string extenderName, object extendeeObject)
        {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
                var selectedItems = await solutionExplorer.GetSelectionAsync();
                var selectedItemList = selectedItems.ToList();

                return selectedItemList.Count == 1
                    && selectedItemList[0].Type == SolutionItemType.Project;
            });
        }
    }
}
