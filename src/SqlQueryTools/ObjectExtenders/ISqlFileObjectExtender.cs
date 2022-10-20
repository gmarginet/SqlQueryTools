using System.Runtime.InteropServices;

namespace SqlQueryTools.ObjectExtenders
{
    [ComVisible(true)]
    public interface ISqlFileObjectExtender
    {
        bool GenerateParameterNames { get; set; }
        bool GeneratePocoClass { get; set; }
    }
}
