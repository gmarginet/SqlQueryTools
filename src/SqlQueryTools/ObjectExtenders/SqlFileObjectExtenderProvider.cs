using EnvDTE;
using SqlQueryTools.Extensions;
using SqlQueryTools.Options;
using System.Linq;
using System.Reflection;

namespace SqlQueryTools.ObjectExtenders
{
    public class SqlFileObjectExtenderProvider : IExtenderProvider
    {
        public object GetExtender(string extenderCatid, string extenderName,
             object extendeeObject, IExtenderSite extenderSite, int cookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var physicalFile = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var options = await GeneralOptions.GetLiveInstanceAsync();

                var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
                var selectedItems = await solutionExplorer.GetSelectionAsync();
                var selectedItemList = selectedItems.ToList();

                return selectedItemList.Count == 1
                    && selectedItemList[0].Type == SolutionItemType.PhysicalFile
                    && selectedItemList[0].Name.EndsWith(options.SqlFileSuffix, StringComparison.InvariantCultureIgnoreCase)
                    ? selectedItemList[0] as PhysicalFile
                    : null;
            });

            return CanExtend(extenderCatid, extenderName, extendeeObject)
                && physicalFile != null
                ? new SqlFileObjectExtender(physicalFile, extenderSite, cookie)
                : null;
        }

        public bool CanExtend(string extenderCatid, string extenderName, object extendeeObject)
        {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var options = await GeneralOptions.GetLiveInstanceAsync();

                var solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
                var selectedItems = await solutionExplorer.GetSelectionAsync();
                var selectedItemList = selectedItems.ToList();

                return selectedItemList.Count == 1
                    && selectedItemList[0].Type == SolutionItemType.PhysicalFile
                    && selectedItemList[0].Name.EndsWith(options.SqlFileSuffix, StringComparison.InvariantCultureIgnoreCase);
            });
        }
    }
}
