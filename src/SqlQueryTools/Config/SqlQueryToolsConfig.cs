namespace SqlQueryTools.Config
{
    public class SqlQueryToolsConfig
    {
        public string ConnectionString { get; set; }
        public string EndOfParameterDeclarationMarker { get; set; }
        public bool GenerateParameterNames { get; set; }
        public bool GenerateResultDtoClass { get; set; }
    }
}