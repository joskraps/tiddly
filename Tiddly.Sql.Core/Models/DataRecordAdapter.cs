namespace Tiddly.Sql.Models
{
    using System;
    using System.Data;

    public sealed class DataRecordAdapter
    {
        private readonly IDataRecord readerRecord;
        private readonly DataRow rowRecord;

        public DataRecordAdapter(IDataRecord row)
        {
            readerRecord = row ?? throw new ArgumentNullException();
        }

        public DataRecordAdapter(DataRow row)
        {
            rowRecord = row ?? throw new ArgumentNullException();
        }

        public object this[int index] => readerRecord != null ? readerRecord[index] : rowRecord[index];
    }
}