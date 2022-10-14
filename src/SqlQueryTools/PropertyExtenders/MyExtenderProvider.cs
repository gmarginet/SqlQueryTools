using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace SqlQueryTools.PropertyExtenders
{
    public class MyExtenderProvider : IExtenderProvider
    {
        private IMyDynamicExtender _extender;
        public object GetExtender(string extenderCatid, string extenderName,
             object extendeeObject, IExtenderSite extenderSite,
            int cookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return _extender = CanExtend(extenderCatid, extenderName, extendeeObject) ?
                new NewPropertyExtender(extenderSite, cookie) : null;
        }

        public bool CanExtend(string extenderCatid, string extenderName, object extendeeObject)
        {
            //((Microsoft.VisualStudio.ProjectSystem.VS.Implementation.PropertyPages.DynamicTypeBrowseObjectBase)extendeeObject).Name

            // Some implementation will be here in the real world. 
            return true;
        }
    }
}
