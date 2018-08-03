using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using Dapper;
using Tiddly.Sql.Core.Tests;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.Models;

namespace Tiddly.Sql.Tests.Performance.DataAccess
{
    [MinColumn, MaxColumn]
    public class Get_Performance_DataSDK
    {
        private const string connectionString = "server=.\\ct;uid=CareTrackerUser;pwd=CareTracker_999;database=ct_test";

        [Benchmark]
        public string Get_FromStatementWithDataReader_WithStringReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            return da.Get<string>(helper);
        }

        [Benchmark]
        public DatabaseModel Get_FromStatementWithDataReader_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            return da.Get<DatabaseModel>(helper);
        }

        //[Benchmark]
        //public void Get_FromStatementWithDataSet_WithStringReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
        //    helper.AddStatement("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    var returnValue = da.Get<string>(helper);

        //}

        //[Benchmark]
        //public void Get_FromStatementWithDataSet_WithObjectReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
        //    helper.AddStatement("select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    var returnValue = da.Get<DatabaseModel>(helper);
        //}

        [Benchmark]
        public void Fill_FromStatementWithDataReader_WithStringReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<string>(helper);
        }

        [Benchmark]
        public void Fill_FromStatementWithDataReader_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<DatabaseModel>(helper);
        }

        //[Benchmark]
        //public void Fill_FromStatementWithDataSet_WithStringReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
        //    helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    var values = da.Fill<string>(helper);
        //}

        //[Benchmark]
        //public void Fill_FromStatementWithDataSet_WithObjectReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
        //    helper.AddStatement("select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    var values = da.Fill<DatabaseModel>(helper);
        //}

        //[Benchmark]
        //public void FillToDictionary_FromStatementWithDataSet_WithObjectReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
        //    helper.AddStatement("select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    var values = da.FillToDictionary<int, DatabaseModel>("Id", helper);
        //}

        //[Benchmark]
        //public void FillToDictionary_FromStatementWithDataReader_WithObjectReturn()
        //{
        //    var da = new SqlDataAccess(connectionString);
        //    var helper = new SqlDataAccessHelper();
        //    helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
        //    helper.AddStatement("select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
        //    helper.AddParameter("master", "master", SqlDbType.VarChar);
        //    helper.AddParameter("model", "model", SqlDbType.VarChar);
        //    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

        //    //TODO is the second type is primitive, it should use row number as the key?
        //    var values = da.FillToDictionary<int, DatabaseModel>("Id", helper);
        //}
    }

    [MinColumn, MaxColumn]
    public class Get_Performance_Dapper
    {
        private const string ConnectionString = "server=.\\ct;uid=CareTrackerUser;pwd=CareTracker_999;database=ct_test";
        private readonly SqlConnection connection;

        public Get_Performance_Dapper()
        {
            connection = new SqlConnection(ConnectionString);
        }
        [Benchmark]
        public object Get_FromStatementWithDataReaderUnBuffered_WithStringReturn()
        {
            return connection.Query("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc", new {master = "master", model = "model", msdb = "msdb"}, buffered: false).First();
        }

        [Benchmark]
        public object Get_FromStatementWithDataReaderBuffered_WithStringReturn()
        {
            return connection.Query("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc", new { master = "master", model = "model", msdb = "msdb" }, buffered: true).First();
        }

        [Benchmark]
        public DatabaseModel Get_FromStatementWithDataReader_WithObjectReturn()
        {
            return connection.Query<DatabaseModel>("select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc"
                , new { master = "master", model = "model", msdb = "msdb" }, buffered: false).First();
        }


        [Benchmark]
        public List<string> Fill_FromStatementWithDataReader_WithStringReturn()
        {
            return connection.Query<string>("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc"
                , new { master = "master", model = "model", msdb = "msdb" }, buffered: false).ToList();
        }

        [Benchmark]
        public List<DatabaseModel> Fill_FromStatementWithDataReader_WithObjectReturn()
        {
            return connection.Query<DatabaseModel>("select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc"
                , new { master = "master", model = "model", msdb = "msdb" }, buffered: false).ToList();
        }
    }
}
