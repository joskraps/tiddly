namespace Tiddly.Sql.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using Sql.Mapping;
    using Models;

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
                        helper.ExecutionContext.ExecutionEvent.DataExecutionTiming = accessTimer.ElapsedTicks;

                        if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
                        {
                            helper.ExecutionContext.ExecutionEvent.DataMappingTiming = -1;
                            return returnList;
                        }

                        // mapper
                        mappingTimer.Start();
                        returnList = SqlMapper.Map<T>(ds.Tables[0], helper.ExecutionContext);
                        mappingTimer.Stop();
                        helper.ExecutionContext.ExecutionEvent.DataMappingTiming = mappingTimer.ElapsedTicks;
                        break;
                    case DataActionRetrievalType.DataReader:
                        accessTimer.Start();
                        var dr = GetDataReader(helper);
                        accessTimer.Stop();
                        helper.ExecutionContext.ExecutionEvent.DataExecutionTiming = accessTimer.ElapsedTicks;

                        if (dr == null || !dr.HasRows)
                        {
                            helper.ExecutionContext.ExecutionEvent.DataMappingTiming = -1;
                            return returnList;
                        }

                        mappingTimer.Start();
                        returnList = SqlMapper.Map<T>(dr, helper.ExecutionContext);
                        mappingTimer.Stop();
                        helper.ExecutionContext.ExecutionEvent.DataMappingTiming = mappingTimer.ElapsedTicks;

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

        public int Execute(SqlDataAccessHelper helper)
        {
            return Get<int>(helper);
        }

        public T Get<T>(SqlDataAccessHelper helper)
        {
            var returnList = Fill<T>(helper);

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
    }
}