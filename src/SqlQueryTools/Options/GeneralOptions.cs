using System.ComponentModel;

namespace SqlQueryTools.Options
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("Sql File")]
        [DisplayName("Sql file sufix")]
        [Description("This suffix will be added tot the filename when SqlQueryTools generates a sql file.")]
        [DefaultValue(".sqt.sql")]
        public string SqlFileSuffix { get; set; } = ".sqt.sql";

        [Category("Sql File")]
        [DisplayName("End of parameter declaration marker")]
        [Description("Marker used in sql files to indicate the end of the parameter declaration.")]
        [DefaultValue("---------- End Of Parameter Declaration ----------")]
        public string EndOfParameterDeclarationMarker { get; set; } = "---------- End Of Parameter Declaration ----------";

        [Category("Code generator")]
        [DisplayName("Class suffix")]
        [Description("This suffix will be added to the class name SqlQueryTools will generate for the sql string.")]
        [DefaultValue("Sql")]
        public string ClassSuffix { get; set; } = "Sql";

        [Category("Code generator")]
        [DisplayName("Code file suffix")]
        [Description("This suffix will be added to the filename when SqlQueryTools generates a code file.")]
        [DefaultValue(".sqt.cs")]
        public string CodeFileSuffix { get; set; } = ".sqt.cs";
    }
}
