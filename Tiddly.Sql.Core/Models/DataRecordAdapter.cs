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
            this.readerRecord = row ?? throw new ArgumentNullException();
        }

        public DataRecordAdapter(DataRow row)
        {
            this.rowRecord = row ?? throw new ArgumentNullException();
        }

        public object this[int index] => this.readerRecord != null ? this.readerRecord[index] : this.rowRecord[index];
    }
}