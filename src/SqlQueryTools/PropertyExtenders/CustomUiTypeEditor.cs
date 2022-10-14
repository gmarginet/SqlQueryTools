using System.ComponentModel;
using System.Drawing.Design;

namespace SqlQueryTools.PropertyExtenders
{
    public class CustomUiTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // Use the result of a dialog or something else here.
            return "HELLO WORLD";
        }
    }
}
