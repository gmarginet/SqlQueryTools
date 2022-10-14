namespace SqlQueryTools.Dto
{
    internal class SpDescribeFirstResultSetResult
    {
        public bool is_hidden { get; set; }
        public int column_ordinal { get; set; }
        public string name { get; set; }
        public bool is_nullable { get; set; }
        public int system_type_id { get; set; }
        public string system_type_name { get; set; }
        public int max_length { get; set; }
        public int precision { get; set; }
        public int scale { get; set; }
        public string collation_name { get; set; }
        public int user_type_id { get; set; }
        public string user_type_database { get; set; }
        public string user_type_schema { get; set; }
        public string user_type_name { get; set; }
        public string assembly_qualified_type_name { get; set; }
        public int xml_collection_id { get; set; }
        public string xml_collection_database { get; set; }
        public string xml_collection_schema { get; set; }
        public string xml_collection_name { get; set; }
        public bool is_xml_document { get; set; }
        public bool is_case_sensitive { get; set; }
        public bool is_fixed_length_clr_type { get; set; }
        public string source_server { get; set; }
        public string source_database { get; set; }
        public string source_schema { get; set; }
        public string source_table { get; set; }
        public string source_column { get; set; }
        public bool is_identity_column { get; set; }
        public bool is_part_of_unique_key { get; set; }
        public bool is_updateable { get; set; }
        public bool is_computed_column { get; set; }
        public bool is_sparse_column_set { get; set; }
        public bool ordinal_in_order_by_list { get; set; }
        public bool order_by_is_descending { get; set; }
        public bool order_by_list_length { get; set; }
        public int tds_type_id { get; set; }
        public int tds_length { get; set; }
        public int tds_collation_id { get; set; }
        public int tds_collation_sort_id { get; set; }
    }
}