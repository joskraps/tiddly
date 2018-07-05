namespace Tiddly.Sql.Models
{
    using System;
    using System.Reflection;

    public class ObjectPropertyMapping
    {
        public string Accessor { get; set; }

        public bool IsEnum { get; set; }

        public bool IsNullable { get; internal set; }

        public string Name { get; set; }

        public PropertyInfo ObjectPropertyInfo { get; set; }

        public Type ObjectType { get; set; }
    }
}