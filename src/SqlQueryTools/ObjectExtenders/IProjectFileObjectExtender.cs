using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SqlQueryTools.ObjectExtenders
{
    [ComVisible(true)]
    public interface IProjectFileObjectExtender
    {
        string ConnectionString { get; set; }
    }
}
