using System;
using System.Data;

namespace Tiddly.Sql.Models
{
    public sealed class DataRecordAdapter
    {
        private readonly IDataRecord readerRecord;
        private readonly DataRow rowRecord;

        public DataRecordAdapter(IDataRecord row)
        {
            readerRecord = row;
        }

        public DataRecordAdapter(DataRow row)
        {
            rowRecord = row;
        }

        public object this[string name]
        {
            get
            {
                if (readerRecord != null)
                    return readerRecord[name];
                if (rowRecord != null)
                    return rowRecord[name];

                throw new DataAdapterNullDataException("BOTH ARE NULL");
            }
        }

        public object this[int index]
        {
            get
            {
                if (readerRecord != null)
                    return readerRecord[index];
                if (rowRecord != null)
                    return rowRecord[index];

                throw new DataAdapterNullDataException("BOTH ARE NULL");
            }
        }

        public T GetValue<T>(string name)
        {
            try
            {
                return (T) this[name];
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }
    }

    public class DataAdapterNullDataException : Exception
    {
        public DataAdapterNullDataException(string message) : base(message)
        {
        }
    }
}