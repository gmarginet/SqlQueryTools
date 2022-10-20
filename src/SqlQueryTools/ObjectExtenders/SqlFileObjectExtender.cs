using EnvDTE;
using SqlQueryTools.Extensions;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SqlQueryTools.ObjectExtenders
{
    [ComVisible(true)]
    public class SqlFileObjectExtender : ISqlFileObjectExtender, IDisposable
    {
        private readonly PhysicalFile physicalFile;
        private readonly IExtenderSite extenderSite;
        private readonly int cookie;
        private bool disposed;

        public SqlFileObjectExtender(PhysicalFile physicalFile, IExtenderSite extenderSite, int cookie)
        {
            this.physicalFile = physicalFile;
            this.extenderSite = extenderSite;
            this.cookie = cookie;
        }

        [DisplayName("Generate Parameter Names")]
        [Category("SqlQueryTools")]
        [Description("Generate Parameter Names property")]
        public bool GenerateParameterNames {
            get => ThreadHelper.JoinableTaskFactory.Run(async () => { return await physicalFile.GetGenerateParameterNamesAsync(); });
            set => ThreadHelper.JoinableTaskFactory.Run(async () => { return await physicalFile.TrySetGenerateParameterNamesAsync(value); });
        }

        [DisplayName("Generate Poco Class")]
        [Category("SqlQueryTools")]
        [Description("Generate Poco Class property")]
        public bool GeneratePocoClass
        {
            get => ThreadHelper.JoinableTaskFactory.Run(async () => { return await physicalFile.GetGeneratePocoClassAsync(); });
            set => ThreadHelper.JoinableTaskFactory.Run(async () => { return await physicalFile.TrySetGeneratePocoClassAsync(value); });
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
