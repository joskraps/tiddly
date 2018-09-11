// ReSharper disable StringLiteralTypo

using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.Models;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class SqlDataAccessTests
    {
        private readonly string ConnectionString = "";

        public SqlDataAccessTests()
        {
            ConnectionString =  ConfigHelper.GetConfigRoot()["DbConnection"];
        }

        [TestMethod]
        public void FillFromStatementWithDataReaderWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0].Name, "master");
            Assert.AreEqual(values[1].Name, "model");
            Assert.AreEqual(values[2].Name, "msdb");
        }

        [TestMethod]
        public void FillFromStatementWithDataReaderWithEnumReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select 0 [EnumType] union all select 1");

            var values = da.Fill<TestClassWithEnum>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 2);
            Assert.IsTrue(values[0].EnumType == TestEnum.Val1);
            Assert.IsTrue(values[1].EnumType == TestEnum.Val2);
        }

        [TestMethod]
        public void FillFromStatementWithDataReaderWithObjectReturnAndCustomMappings()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select name [Map1],database_id [Map2],is_read_only [Map3],service_broker_guid [Map4],create_date [Map5]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar)
                .AddParameter("model", "model", SqlDbType.VarChar)
                .AddParameter("msdb", "msdb", SqlDbType.VarChar)
                .AddCustomMapping("Map1", "Name")
                .AddCustomMapping("Map2", "Id")
                .AddCustomMapping("Map3", "ReadOnly")
                .AddCustomMapping("Map4", "BrokerGuid")
                .AddCustomMapping("Map5", "CreateDate");

            var values = da.Fill<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0].Name, "master");
            Assert.AreEqual(values[1].Name, "model");
            Assert.AreEqual(values[2].Name, "msdb");
        }

        [TestMethod]
        public void FillFromProcedureWithDataReaderWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddProcedure("sp_help");

            var values = da.Fill<DbHelpModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.IsTrue(values.Count > 0);
        }

        [TestMethod]
        public void FillFromProcedureWithDataReaderWithObjectReturnBadProcedureName()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddProcedure("sp_helpxxx");

            try
            {
                var values = da.Fill<DbHelpModel>(helper);
                Assert.Fail("Procedure does not exist and should throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "Could not find stored procedure \'dbo.sp_helpxxx\'.");
            }
        }

        [TestMethod]
        public void ConnectionTimeoutTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.SetTimeout(1);
            helper.AddStatement("WAITFOR DELAY \'00:15\';");

            try
            {
                var values = da.Fill<DbHelpModel>(helper);
                Assert.Fail("Procedure does not exist and should throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message == "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.");
            }
        }

        [TestMethod]
        public void FillFromStatementWithDataReaderWithStringReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<string>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0], "master");
            Assert.AreEqual(values[1], "model");
            Assert.AreEqual(values[2], "msdb");
        }

        [TestMethod]
        public void FillFromStatementWithDataSetWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<DatabaseModel>(helper);

            this.OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0].Name, "master");
            Assert.AreEqual(values[1].Name, "model");
            Assert.AreEqual(values[2].Name, "msdb");
        }

        [TestMethod]
        public void FillFromStatementWithDataSetWithStringReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<string>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(values.Count, 3);
            Assert.AreEqual(values[0], "master");
            Assert.AreEqual(values[1], "model");
            Assert.AreEqual(values[2], "msdb");
        }

        [TestMethod]
        public void FillToDictionaryFromStatementWithDataReaderWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            // TODO is the second type is primitive, it should use row number as the key?
            var values = da.FillToDictionary<int, DatabaseModel>("Id", helper);

            this.OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Keys.Count, 3);
            Assert.AreEqual(values[1].Name, "master");
            Assert.AreEqual(values[2].Name, "model");
            Assert.AreEqual(values[3].Name, "msdb");
        }

        [TestMethod]
        public void FillToDictionaryFromStatementWithDataSetWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.FillToDictionary<int, DatabaseModel>("Id", helper);

            this.OutputTestTimings(helper.ExecutionContext);

            Assert.AreEqual(values.Keys.Count, 3);
            Assert.AreEqual(values[1].Name, "master");
            Assert.AreEqual(values[2].Name, "model");
            Assert.AreEqual(values[3].Name, "msdb");
        }

        [TestMethod]
        public void GetFromStatementWithDataReaderWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();

            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "master");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
        public void GetNullableTypesFromStatementWithReaderTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select null from sys.databases where name in (@master) union all select 1 from sys.databases where name in (@master)  order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);

            var returnValue = da.Fill<int?>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.IsTrue(returnValue[0] == null);
            Assert.IsTrue(returnValue[1] == 1);
        }

        [TestMethod]
        public void GetNullableTypesFromStatementWithDataSetTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select null from sys.databases where name in (@master) union all select 1 from sys.databases where name in (@master)  order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);

            var returnValue = da.Fill<int?>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.IsTrue(returnValue[0] == null);
            Assert.IsTrue(returnValue[1] == 1);
        }

        [TestMethod]
        public void GetFromStatementWithDataReaderWithStringReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var getValue = da.Get<string>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(getValue, "master");
        }

        [TestMethod]
        public void GetFromStatementWithDataSetWithObjectReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "master");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
        public void GetFromStatementWithDataSetTableSchmeaMapping()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select top 1 name [usp_Name],database_id [usp_Id],is_read_only [usp_ReadOnly],service_broker_guid [usp_BrokerGuid],create_date [usp_CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);
            helper.ExecutionContext.TableSchema = "usp_";

            var returnValue = da.Get<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "master");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
        public void GetFromStatementWithDataSetWithStringReturn()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();

            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var returnValue = da.Get<string>(helper);

            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue, "master");
        }

        [TestMethod]
        public void PostProcessingTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);
            helper.SetPostProcessFunction<string>("name", s => s == "master" ? "MAPPING FUNCTION" : s);

            var returnValue = da.Get<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "MAPPING FUNCTION");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
        }

        [TestMethod]
        public void AddParameterWithNullOrEmptyNameShouldThrowException()
        {
            try
            {
                var helper = new SqlDataAccessHelper();
                helper.AddParameter(string.Empty, string.Empty, SqlDbType.VarChar);
                Assert.Fail("Should throw an exception because of an empty name");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(ArgumentException));
                Assert.IsTrue(e.Message == "name cannot be null or empty.");
            }
        }

        [TestMethod]
        public void ScrubbingParameterValueTest()
        {
            var helper = new SqlDataAccessHelper();
            helper.AddParameter("Test", "select name from sys.databases --boom", SqlDbType.VarChar, true);

            Assert.IsTrue(helper.Property("test").Value.ToString() == "select name from sys.databases boom");
        }

        [TestMethod]
        public void ScrubbingIsValidTest()
        {
            var isValid = SqlScrubber.IsValid("select name from sys.databases --boom");
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void EmptyConnectionStringTest()
        {
            try
            {
                var da = new SqlDataAccess(string.Empty);
                Assert.Fail("Should throw an exception because of the empty connection string");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("Connection string is not valid: "));
            }
        }

        [TestMethod]
        public void EmptyDataSetReturnShouldReturnAnEmptyCollectionTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in ('testNotThere') order by 1 asc");

            var values = da.Fill<DatabaseModel>(helper);

            this.OutputTestTimings(helper.ExecutionContext);
            
            Assert.IsTrue(values.Count == 0);
        }

        [TestMethod]
        public void EmptyDataReaderReturnShouldReturnAnEmptyCollectionTest()
        {
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
            helper.AddStatement(
                "select name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate]  from sys.databases where name in ('testNotThere') order by 1 asc");

            var values = da.Fill<DatabaseModel>(helper);

            this.OutputTestTimings(helper.ExecutionContext);

            Assert.IsTrue(values.Count == 0);
        }

        [TestMethod]
        public void ServerNameShouldReturnServerName()
        {
            var da = new SqlDataAccess(ConnectionString);
            var serverName = da.GetServerName();

            Assert.IsTrue(serverName.Length > 0);
        }

        private void OutputTestTimings(ExecutionContext helperExecutionContext)
        {
            Console.WriteLine(
                $"Data execution time: {helperExecutionContext.ExecutionEvent.DataExecutionTiming} Data mapping time: {helperExecutionContext.ExecutionEvent.DataMappingTiming} Property mapping timing: {helperExecutionContext.ExecutionEvent.GeneratePropertiesTiming} Propert indices timing: {helperExecutionContext.ExecutionEvent.GeneratePropertyIndicesTiming}");
        }

        private class TestClassWithEnum
        {
            public TestEnum EnumType { get; set; }
        }

        private enum TestEnum
        {
            Val1 = 0,
            Val2 = 1
        }

        private class DbHelpModel
        {
            public string Name { get; set; }

            public string Owner { get; set; }

            public string ObjectType { get; set; }
        }
    }
}