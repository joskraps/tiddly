namespace Tiddly.Sql.Models
{
    using System.Data;

    public class ParameterMapping
    {
        public SqlDbType DataType { get; set; }

        public string Name { get; set; }

        public bool ScrubValue { get; set; }

        public object Value { get; set; }
    }
}