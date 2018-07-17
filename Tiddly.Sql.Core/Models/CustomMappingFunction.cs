namespace Tiddly.Sql.Models
{
    using System;

    public class CustomMappingFunction
    {
        public Func<string, object> Action { get; set; }

        public string TargetProperty { get; set; }

        public Type TargetType { get; set; }
    }
}