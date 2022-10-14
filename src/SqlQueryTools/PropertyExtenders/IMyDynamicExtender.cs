using System.Runtime.InteropServices;

namespace SqlQueryTools.PropertyExtenders
{
    [ComVisible(true)]
    public interface IMyDynamicExtender
    {
        String NewProperty { get; set; }
    }
}
