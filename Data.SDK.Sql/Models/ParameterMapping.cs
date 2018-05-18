using System;
using System.Data;

namespace Tiddly.Sql.Models
{
    public class ParameterMapping
    {
        public SqlDbType DataType { get; set; }
        public string Name { get; set; }
        public bool ScrubValue { get; set; }
        public Type TargeType { get; set; }
        public object Value { get; set; }
    }
}