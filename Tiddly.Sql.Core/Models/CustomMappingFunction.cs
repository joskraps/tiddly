namespace Tiddly.Sql.Models
{
    using System;

    public class CustomMappingFunction
    {
        public Func<object, object> Action { get; set; }

        public string TargetProperty { get; set; }

        public Type TargetType { get; set; }
    }
}