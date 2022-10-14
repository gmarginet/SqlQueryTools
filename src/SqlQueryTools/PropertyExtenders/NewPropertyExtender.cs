using EnvDTE;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace SqlQueryTools.PropertyExtenders
{
    [ComVisible(true)] // Important!
    public class NewPropertyExtender : IMyDynamicExtender, IDisposable
    {
        // These attibutes supply the property with some information
        // on how to display and which UITypeEditor to use.
        [DisplayName("New Property")]
        [Category("New")]
        [Description("Specifies the new property")]
        //[Editor(typeof(CustomUiTypeEditor), typeof(UITypeEditor))]
        public String NewProperty { get; set; }
        private readonly IExtenderSite _extenderSite;
        private readonly int _cookie;
        private bool _disposed;

        public NewPropertyExtender(IExtenderSite extenderSite, int cookie)
        {
            _extenderSite = extenderSite;
            _cookie = cookie;
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

            if (_disposed) return;
            if (disposing && _cookie != 0)
            {
                _extenderSite.NotifyDelete(_cookie);
            }
            _disposed = true;
        }
    }
}
