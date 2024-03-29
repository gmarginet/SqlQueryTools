﻿using System.ComponentModel;

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
        [DisplayName("Default generate parameter names")]
        [Description("This default is used when a new sql file is created. It marks that the parameter name fields should be generated for this sql file.")]
        [DefaultValue(true)]
        public bool GenerateParameterNames { get; set; } = true;

        [Category("Sql File")]
        [DisplayName("Default generate poco class")]
        [Description("This default is used when a new sql file is created. It marks that poco class should be generated for this sql file. The poco class contains properties for every column that gets returned by the query.")]
        [DefaultValue(false)]
        public bool GeneratePocoClass { get; set; } = false;

        [Category("Code generator")]
        [DisplayName("Class suffix")]
        [Description("This suffix will be added to the class name SqlQueryTools will generate for the sql string.")]
        [DefaultValue("Sql")]
        public string ClassSuffix { get; set; } = "Sql";

        [Category("Code generator")]
        [DisplayName("Sql String Field Name")]
        [Description("This name will be used for the const field in the class SqlQueryTools will generate for the sql string.")]
        [DefaultValue("Query")]
        public string SqlStringFieldName { get; set; } = "Query";

        [Category("Code generator")]
        [DisplayName("Code file suffix")]
        [Description("This suffix will be added to the filename when SqlQueryTools generates a code file.")]
        [DefaultValue(".sqt.cs")]
        public string CodeFileSuffix { get; set; } = ".sqt.cs";

        [Category("Code generator")]
        [DisplayName("Poco Class suffix")]
        [Description("This suffix will be added to the poco class name SqlQueryTools will generate.")]
        [DefaultValue("Poco")]
        public string PocoClassSuffix { get; set; } = "Poco";
    }
}
