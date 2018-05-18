using System;

namespace Tiddly.Sql.Models
{
    public class CustomMappingFunction
    {
        public Func<string, object> Action { get; set; }
        public string TargetProperty { get; set; }
        public Type TargetType { get; set; }
    }
}