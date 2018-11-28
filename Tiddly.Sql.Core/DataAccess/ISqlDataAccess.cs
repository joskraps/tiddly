using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

        Task<int> ExecuteAsync(SqlDataAccessHelper helper);
        Task<IList<T>> FillAsync<T>(SqlDataAccessHelper helper);
        Task<IDictionary<TKey, TObjType>> FillToDictionaryAsync<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false);
        Task<T> GetAsync<T>(SqlDataAccessHelper helper);
        Task<SqlDataReader> GetDataReaderAsync(SqlDataAccessHelper helper);
        Task<DataSet> GetDataSetAsync(SqlDataAccessHelper helper);
    }
}