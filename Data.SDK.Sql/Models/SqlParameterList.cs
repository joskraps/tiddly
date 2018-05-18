using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Tiddly.Sql.Models
{
    public class SqlParameterList : IList<SqlParameter>
    {
        private readonly List<SqlParameter> list;

        public SqlParameterList()
        {
            list = new List<SqlParameter>();
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public SqlParameter this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public int IndexOf(SqlParameter item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, SqlParameter item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void Add(SqlParameter item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(SqlParameter item)
        {
            return list.Contains(item);
        }

        public void CopyTo(SqlParameter[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(SqlParameter item)
        {
            return list.Remove(item);
        }

        public IEnumerator<SqlParameter> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(string name, object value, SqlDbType dataType)
        {
            list.Add(new SqlParameter {ParameterName = name, Value = value ?? DBNull.Value, SqlDbType = dataType});
        }

        public void Add(string name, object value)
        {
            Add(name, value, DbTypeLookup(value));
        }

        private static SqlDbType DbTypeLookup(object value)
        {
            switch (value)
            {
                case string _:
                    return SqlDbType.VarChar;
                case Guid _:
                    return SqlDbType.UniqueIdentifier;
                case decimal _:
                    return SqlDbType.Decimal;
                case double _:
                    return SqlDbType.Float;
                case float _:
                    return SqlDbType.Real;
                case short _:
                    return SqlDbType.SmallInt;
                case int _:
                    return SqlDbType.Int;
                case long _:
                    return SqlDbType.BigInt;
                case bool _:
                    return SqlDbType.Bit;
                case DateTime _:
                    return SqlDbType.DateTime;
                case DateTimeOffset _:
                    return SqlDbType.DateTimeOffset;
                case DataTable _:
                    return SqlDbType.Structured;
                case byte _:
                    return SqlDbType.VarBinary;
                case byte[] _:
                    return SqlDbType.Image;
                default:
                    throw new ArgumentException($"Data type not mapped! Name: {value.GetType().FullName}.");
            }
        }
    }
}