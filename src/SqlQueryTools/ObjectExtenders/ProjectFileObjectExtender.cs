using EnvDTE;
using SqlQueryTools.Extensions;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Project = Community.VisualStudio.Toolkit.Project;

namespace SqlQueryTools.ObjectExtenders
{
    [ComVisible(true)]
    public class ProjectFileObjectExtender : IProjectFileObjectExtender, IDisposable
    {
        private readonly Project project;
        private readonly IExtenderSite extenderSite;
        private readonly int cookie;
        private bool disposed;

        public ProjectFileObjectExtender(Project project, IExtenderSite extenderSite, int cookie)
        {
            this.project = project;
            this.extenderSite = extenderSite;
            this.cookie = cookie;
        }

        [DisplayName("Connection String")]
        [Category("SqlQueryTools")]
        [Description("Connection String property")]
        public string ConnectionString {
            get => ThreadHelper.JoinableTaskFactory.Run(async () => { return await project.GetConnectionStringAsync(); });
            set => ThreadHelper.JoinableTaskFactory.Run(async () => { return await project.TrySetConnectionStringAsync(value); });
        }

        public void Dispose()
        {
            Dispose(true);
            // take the instance off of the finalization queue.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (disposed) return;
            if (disposing && cookie != 0)
            {
                extenderSite.NotifyDelete(cookie);
            }
            disposed = true;
        }
    }
}
