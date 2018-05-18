using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.Models;

namespace Tiddly.Sql.Tests
{
    public class DatabaseModel
    {
        public string Name { get; set; }
        public Guid BrokerGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public int Id { get; set; }
        public bool ReadOnly { get; set; }

    }
    [TestClass]
    public class SqlDataAccessTests
    {
        private const string connectionString = "server=.\\ct;uid=CareTrackerUser;pwd=CareTracker_999;database=master";

        [TestMethod]
        public void Get_FromStatementWithDataReader_WithStringReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var getValue = da.Get<string>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(getValue, "master");
        }

        [TestMethod]
        public void Get_FromStatementWithDataReader_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<DatabaseModel>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "master");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
        public void Get_FromStatementWithDataSet_WithStringReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<string>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue, "master");
            
        }

        [TestMethod]
        public void Get_FromStatementWithDataSet_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<DatabaseModel>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "master");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
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
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0], "master");
            Assert.AreEqual(values[1], "model");
            Assert.AreEqual(values[2], "msdb");
        }

        [TestMethod]
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
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0].Name, "master");
            Assert.AreEqual(values[1].Name, "model");
            Assert.AreEqual(values[2].Name, "msdb");
        }

        [TestMethod]
        public void Fill_FromStatementWithDataSet_WithStringReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<string>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0], "master");
            Assert.AreEqual(values[1], "model");
            Assert.AreEqual(values[2], "msdb");
        }

        [TestMethod]
        public void Fill_FromStatementWithDataSet_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<DatabaseModel>(helper);

            OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0].Name, "master");
            Assert.AreEqual(values[1].Name, "model");
            Assert.AreEqual(values[2].Name, "msdb");
        }

        [TestMethod]
        public void FillToDictionary_FromStatementWithDataSet_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.FillToDictionary<int,DatabaseModel>("Id",helper);

            OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Keys.Count, 3);
            Assert.AreEqual(values[1].Name, "master");
            Assert.AreEqual(values[2].Name, "model");
            Assert.AreEqual(values[3].Name, "msdb");
        }

        [TestMethod]
        public void FillToDictionary_FromStatementWithDataReader_WithObjectReturn()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            //TODO is the second type is primitive, it should use row number as the key?
            var values = da.FillToDictionary<int, DatabaseModel>("Id", helper);
            var boom = helper.ExecutionContext;

            OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Keys.Count, 3);
            Assert.AreEqual(values[1].Name, "master");
            Assert.AreEqual(values[2].Name, "model");
            Assert.AreEqual(values[3].Name, "msdb");
        }

        [TestMethod]
        public void PostProcessing_Test()
        {
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);
            helper.SetPostProcessFunction<string>("name", s => s == "master" ? "MAPPING FUNCTION" : s);

            var returnValue = da.Get<DatabaseModel>(helper);
            OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "MAPPING FUNCTION");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        private void OutputTestTimings(ExecutionContext helperExecutionContext)
        {
            Console.WriteLine($"Data execution time: {helperExecutionContext.ExecutionEvent.DataExecutionTiming} Data mapping time: {helperExecutionContext.ExecutionEvent.DataMappingTiming} Property mapping timing: {helperExecutionContext.ExecutionEvent.GeneratePropertiesTiming} Propert indices timing: {helperExecutionContext.ExecutionEvent.GeneratePropertyIndicesTiming}");
        }


    }
}
