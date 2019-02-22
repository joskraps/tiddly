using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Tiddly.Sql.DataAccess
{
    public sealed class EnhancedSqlDataReader : IDataReader
    {
        private readonly IDataReader dr;

        public EnhancedSqlDataReader(IDataReader dataReader)
        {
            dr = dataReader;
        }

        public int RecordsAffected => dr.RecordsAffected;

        public int Depth => dr.Depth;

        public bool IsClosed => dr.IsClosed;

        public int FieldCount => dr.FieldCount;

        public object this[string name] => dr[name] == DBNull.Value ? null : dr[name];

        public object this[int i] => dr[i] == DBNull.Value ? null : dr[i];

        public void Close()
        {
            dr.Close();
        }

        public DataTable GetSchemaTable()
        {
            return dr.GetSchemaTable();
        }

        public bool NextResult()
        {
            return dr.NextResult();
        }

        public bool Read()
        {
            return dr.Read();
        }

        public void Dispose()
        {
            dr.Dispose();
        }

        public bool GetBoolean(int i)
        {
            return dr.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return dr.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return dr.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return dr.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return dr.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return dr.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return dr.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return dr.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return dr.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return dr.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return dr.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return dr.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            if (dr.GetFieldType(i) == typeof(string))
            {
                return Guid.Parse(dr.GetString(i));
            }

            return dr.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return dr.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return dr.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return dr.GetInt64(i);
        }

        public string GetName(int i)
        {
            return dr.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return dr.GetOrdinal(name);
        }

        public string GetString(int i)
        {
            return dr.GetString(i);
        }

        public object GetValue(int i)
        {
            return dr.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return dr.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return dr.IsDBNull(i);
        }

        public bool GetBoolean(string name)
        {
            return dr.GetBoolean(dr.GetOrdinal(name));
        }

        public byte GetByte(string name)
        {
            return dr.GetByte(dr.GetOrdinal(name));
        }

        public DateTime GetDateTime(string name)
        {
            return dr.GetDateTime(dr.GetOrdinal(name));
        }

        public DateTime? GetNullableDateTime(string name)
        {
            return dr.IsDBNull(dr.GetOrdinal(name)) ? (DateTime?)null : dr.GetDateTime(dr.GetOrdinal(name));
        }

        public decimal GetDecimal(string name)
        {
            return dr.GetDecimal(dr.GetOrdinal(name));
        }

        public double GetDouble(string name)
        {
            return dr.GetDouble(dr.GetOrdinal(name));
        }

        public float GetFloat(string name)
        {
            return dr.GetFloat(dr.GetOrdinal(name));
        }

        public Guid GetGuid(string name)
        {
            return GetGuid(dr.GetOrdinal(name));
        }

        public short GetInt16(string name)
        {
            return dr.GetInt16(dr.GetOrdinal(name));
        }

        public int GetInt32(string name)
        {
            return dr.GetInt32(dr.GetOrdinal(name));
        }

        public long GetInt64(string name)
        {
            return dr.GetInt64(dr.GetOrdinal(name));
        }

        public bool ColumnExists(string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i) == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetString(string name)
        {
            return dr.GetString(dr.GetOrdinal(name));
        }

        public string GetStringValueOrEmpty(string name)
        {
            return (!dr.IsDBNull(dr.GetOrdinal(name)) ? dr.GetString(dr.GetOrdinal(name)) : string.Empty);
        }

        public bool IsDBNull(string name)
        {
            return dr.IsDBNull(GetOrdinal(name));
        }
    }
}
