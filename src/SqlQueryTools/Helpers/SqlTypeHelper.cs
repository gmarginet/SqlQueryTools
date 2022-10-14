namespace SqlQueryTools.Helpers
{
    internal static class SqlTypeHelper
    {
        internal static string ToCSharpTypeName(string sqlTypeName)
        {
            if (sqlTypeName.ToLower().StartsWith("bigint")) return "System.Int64";
            if (sqlTypeName.ToLower().StartsWith("binary")) return "System.Byte[]";
            if (sqlTypeName.ToLower().StartsWith("bit")) return "System.Boolean";
            if (sqlTypeName.ToLower().StartsWith("char")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("datetimeoffset")) return "System.DateTimeOffset";
            if (sqlTypeName.ToLower().StartsWith("datetime2")) return "System.DateTime";
            if (sqlTypeName.ToLower().StartsWith("datetime")) return "System.DateTime";
            if (sqlTypeName.ToLower().StartsWith("date")) return "System.DateTime";
            if (sqlTypeName.ToLower().StartsWith("decimal")) return "System.Decimal";
            if (sqlTypeName.ToLower().StartsWith("float")) return "System.Double";
            if (sqlTypeName.ToLower().StartsWith("image")) return "System.Byte[]";
            if (sqlTypeName.ToLower().StartsWith("int")) return "System.Int32";
            if (sqlTypeName.ToLower().StartsWith("money")) return "System.Decimal";
            if (sqlTypeName.ToLower().StartsWith("nchar")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("ntext")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("numeric")) return "System.Decimal";
            if (sqlTypeName.ToLower().StartsWith("nvarchar")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("real")) return "System.Single";
            if (sqlTypeName.ToLower().StartsWith("rowversion")) return "System.Byte[]";
            if (sqlTypeName.ToLower().StartsWith("smalldatetime")) return "System.DateTime";
            if (sqlTypeName.ToLower().StartsWith("smallint")) return "System.Int16";
            if (sqlTypeName.ToLower().StartsWith("smallmoney")) return "System.Decimal";
            if (sqlTypeName.ToLower().StartsWith("sql_variant")) return "System.Object";
            if (sqlTypeName.ToLower().StartsWith("text")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("timestamp")) return "System.Byte[]";
            if (sqlTypeName.ToLower().StartsWith("time")) return "System.TimeSpan";
            if (sqlTypeName.ToLower().StartsWith("tinyint")) return "System.Byte";
            if (sqlTypeName.ToLower().StartsWith("uniqueidentifier")) return "System.Guid";
            if (sqlTypeName.ToLower().StartsWith("varbinary")) return "System.Byte[]";
            if (sqlTypeName.ToLower().StartsWith("varchar")) return "System.String";
            if (sqlTypeName.ToLower().StartsWith("xml")) return "System.Xml";

            return "System.Object";
        }
    }
}
