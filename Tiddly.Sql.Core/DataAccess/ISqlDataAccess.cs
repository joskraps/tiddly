using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Tiddly.Sql.DataAccess
{
    public interface ISqlDataAccess
    {
        SqlConnection Connection { get; }
        int Execute(SqlDataAccessHelper helper);
        IList<T> Fill<T>(SqlDataAccessHelper helper);
        IDictionary<TKey, TObjType> FillToDictionary<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false);
        T Get<T>(SqlDataAccessHelper helper);
        SqlDataReader GetDataReader(SqlDataAccessHelper helper);
        DataSet GetDataSet(SqlDataAccessHelper helper);
        string GetServerName();
    }
}