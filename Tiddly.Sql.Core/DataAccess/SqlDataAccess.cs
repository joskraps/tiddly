namespace Tiddly.Sql.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using Sql.Mapping;
    using Models;
    using System.Threading.Tasks;

    // ReSharper disable once UnusedMember.Global
    public class SqlDataAccess : ISqlDataAccess
    {
        public SqlDataAccess(string connectionString) : this(connectionString, null)
        {
        }

        public SqlDataAccess(string connectionString, ISqlUnitOfWork unitOfWork)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Connection string is not valid: {connectionString}");
            }

            Connection = new SqlConnection(connectionString);

            if (unitOfWork != null)
            {
                UnitOfWork = unitOfWork;
            }
        }

        public SqlConnection Connection { get; }

        public ISqlUnitOfWork UnitOfWork { get; }

        public IList<T> Fill<T>(SqlDataAccessHelper helper)
        {
            var returnList = new List<T>();
            var accessTimer = new Stopwatch();
            var mappingTimer = new Stopwatch();

            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (helper.ExecutionContext.DataRetrievalType)
                {
                    case DataActionRetrievalType.DataSet:
                        accessTimer.Start();
                        var ds = GetDataSet(helper);
                        accessTimer.Stop();

                        MassageResults(ref helper, ref ds, ref returnList, ref accessTimer, ref mappingTimer);
                        break;
                    case DataActionRetrievalType.DataReader:
                        accessTimer.Start();
                        var dr = GetDataReader(helper);
                        accessTimer.Stop();

                        MassageResults(ref helper, dr, ref returnList, ref accessTimer, ref mappingTimer);
                        break;
                }

                return returnList;
            }
            catch (Exception e)
            {
                helper.ExecutionContext.ExecutionEvent.ExecutionErrors.Add(e);
                throw;
            }
        }
        public async Task<IList<T>> FillAsync<T>(SqlDataAccessHelper helper)
        {
            var returnList = new List<T>();
            var accessTimer = new Stopwatch();
            var mappingTimer = new Stopwatch();

            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (helper.ExecutionContext.DataRetrievalType)
                {
                    case DataActionRetrievalType.DataSet:
                        accessTimer.Start();
                        var ds = await GetDataSetAsync(helper);
                        accessTimer.Stop();

                        MassageResults(ref helper, ref ds, ref returnList, ref accessTimer, ref mappingTimer);

                        break;
                    case DataActionRetrievalType.DataReader:
                        accessTimer.Start();
                        var dr = GetDataReaderAsync(helper);
                        accessTimer.Stop();

                        MassageResults(ref helper, dr.Result, ref returnList, ref accessTimer, ref mappingTimer);
                        break;
                }

                return returnList;
            }
            catch (Exception e)
            {
                helper.ExecutionContext.ExecutionEvent.ExecutionErrors.Add(e);
                throw;
            }
        }

        public IDictionary<TKey, TObjType> FillToDictionary<TKey, TObjType>(
            string keyPropertyName,
            SqlDataAccessHelper helper,
            bool overwriteOnDupe = false)
        {
            var initialReturn = Fill<TObjType>(helper);

            var returnList =
                SqlMapper.KeyedMap<TKey, TObjType>(keyPropertyName, initialReturn, helper.ExecutionContext, false);

            return returnList;
        }
        public async Task<IDictionary<TKey, TObjType>> FillToDictionaryAsync<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false)
        {
            var initialReturn = await FillAsync<TObjType>(helper);

            var returnList =
                SqlMapper.KeyedMap<TKey, TObjType>(keyPropertyName, initialReturn, helper.ExecutionContext, false);

            return returnList;
        }

        public int Execute(SqlDataAccessHelper helper)
        {
            return Get<int>(helper);
        }
        public async Task<int> ExecuteAsync(SqlDataAccessHelper helper)
        {
            return await GetAsync<int>(helper);
        }

        public T Get<T>(SqlDataAccessHelper helper)
        {
            var returnList = Fill<T>(helper);

            return returnList.Count == 0 ? default(T) : returnList[0];
        }
        public async Task<T> GetAsync<T>(SqlDataAccessHelper helper)
        {
            var returnList = await FillAsync<T>(helper);

            return returnList.Count == 0 ? default(T) : returnList[0];
        }

        public SqlDataReader GetDataReader(SqlDataAccessHelper helper)
        {
            var cmd = GetCommand(helper);

            PrepareConnection();

            return cmd.ExecuteReader(
                UnitOfWork != null && UnitOfWork.CurrentlyOpen
                ? CommandBehavior.Default
                : CommandBehavior.CloseConnection);
        }
        public async Task<SqlDataReader> GetDataReaderAsync(SqlDataAccessHelper helper)
        {
            var cmd = GetCommand(helper);

            await PrepareConnectionAsync();

            return await cmd.ExecuteReaderAsync(
                UnitOfWork != null && UnitOfWork.CurrentlyOpen
                ? CommandBehavior.Default
                : CommandBehavior.CloseConnection);
        }

        public EnhancedSqlDataReader GetEnhancedDataReader(SqlDataAccessHelper helper)
        {
            var cmd = GetCommand(helper);

            PrepareConnection();

            return new EnhancedSqlDataReader(cmd.ExecuteReader(CommandBehavior.CloseConnection));
        }

        public async Task<EnhancedSqlDataReader> GetEnhancedDataReaderAsync(SqlDataAccessHelper helper)
        {
            var cmd = GetCommand(helper);

            PrepareConnection();

            var results = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            return new EnhancedSqlDataReader(results);
        }

        public DataSet GetDataSet(SqlDataAccessHelper helper)
        {
            var ds = new DataSet();
            var sqlCommand = GetCommand(helper);
            SqlDataAdapter sqlDa = null;

            try
            {
                sqlDa = new SqlDataAdapter(sqlCommand);

                PrepareConnection();

                sqlDa.Fill(ds);
            }
            finally
            {
                FinalizeConnection();

                sqlDa?.Dispose();
            }

            return ds;
        }
        public async Task<DataSet> GetDataSetAsync(SqlDataAccessHelper helper)
        {
            var reader = await GetDataReaderAsync(helper);
            var ds = new DataSet();

            while (!reader.IsClosed)
            {
                var dt = new DataTable();
                dt.Load(reader);

                ds.Tables.Add(dt);
            };

            return ds;

        }

        public string GetServerName()
        {
            var returnAccess = new SqlDataAccessHelper();

            returnAccess.AddStatement("SELECT @@SERVERNAME");

            return Get<string>(returnAccess);
        }   

        private void FinalizeConnection()
        {
            if (Connection.State != ConnectionState.Closed
                && (UnitOfWork == null || UnitOfWork.CurrentlyOpen == false))
            {
                Connection.Close();
            }
        }

        private SqlCommand GetCommand(SqlDataAccessHelper helper)
        {
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.CommandTimeout = helper.ExecutionContext.Timeout;

            if (UnitOfWork != null && UnitOfWork.CurrentlyOpen)
            {
                sqlCommand.Transaction = UnitOfWork.GetSqlTransaction(Connection.ConnectionString);
            }

            sqlCommand.CommandType = CommandType.Text;

            switch (helper.ExecutionContext.ActionType)
            {
                case DataAccessActionType.Statement:
                    sqlCommand.CommandText = helper.ExecutionContext.Statement;
                    break;
                case DataAccessActionType.Procedure:
                    sqlCommand.CommandText = helper.ExecutionContext.ProcedureName;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    break;
            }

            foreach (var parameter in helper.ExecutionContext.ParameterMappings.Values)
            {
                sqlCommand.Parameters.Add(
                    new SqlParameter
                    {
                        ParameterName = parameter.Name,
                        Value = parameter.Value ?? DBNull.Value,
                        SqlDbType = parameter.DataType
                    });
            }

            return sqlCommand;
        }

        private void PrepareConnection()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        private async Task PrepareConnectionAsync()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }
        }

        private void MassageResults<T>(ref SqlDataAccessHelper helper, SqlDataReader dr, ref List<T> returnList, ref Stopwatch accessTimer, ref Stopwatch mappingTimer)
        {
            helper.ExecutionContext.ExecutionEvent.DataExecutionTiming = accessTimer.ElapsedTicks;

            if (dr == null || !dr.HasRows)
            {
                helper.ExecutionContext.ExecutionEvent.DataMappingTiming = -1;
                return;
            }

            mappingTimer.Start();
            returnList = SqlMapper.Map<T>(dr, helper.ExecutionContext);
            mappingTimer.Stop();
            helper.ExecutionContext.ExecutionEvent.DataMappingTiming = mappingTimer.ElapsedTicks;
        }

        private void MassageResults<T>(ref SqlDataAccessHelper helper, ref DataSet ds, ref List<T> returnList, ref Stopwatch accessTimer, ref Stopwatch mappingTimer)
        {
            helper.ExecutionContext.ExecutionEvent.DataExecutionTiming = accessTimer.ElapsedTicks;

            if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
            {
                helper.ExecutionContext.ExecutionEvent.DataMappingTiming = -1;
                return;
            }

            // mapper
            mappingTimer.Start();
            returnList = SqlMapper.Map<T>(ds.Tables[0], helper.ExecutionContext);
            mappingTimer.Stop();
            helper.ExecutionContext.ExecutionEvent.DataMappingTiming = mappingTimer.ElapsedTicks;
        }
    }
}