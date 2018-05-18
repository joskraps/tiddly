using System;

namespace Tiddly.Sql.Models
{
    public class CustomValueFormatter
    {
        public Func<object, object> Action { get; set; }
        public Type SourceType { get; set; }
        public string TargetPropertyName { get; set; }
        public Type TargetType { get; set; }
    }
}