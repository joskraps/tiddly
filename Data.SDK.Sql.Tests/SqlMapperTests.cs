using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.Mapping;
using Tiddly.Sql.Models;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class SqlMapperTests
    {
        [TestMethod]
        public void TestDataTypeConversions()
        {
            var testDt = DateTime.UtcNow;
            var testGuid = Guid.NewGuid();

            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("IntegerValueNullable", typeof(int)),
                new DataColumn("StringValue1", typeof(string)),
                new DataColumn("DateTimeValue1", typeof(DateTime)),
                new DataColumn("DateTimeValueNullable", typeof(DateTime)),
                new DataColumn("FloatValue", typeof(float)),
                new DataColumn("GuidValue", typeof(Guid)),
                new DataColumn("DecimalValue", typeof(decimal)),
                new DataColumn("CharValue", typeof(char))
            });

            var dr = dt.NewRow();
            dr["IntegerValue1"] = 69;
            dr["IntegerValueNullable"] = DBNull.Value;
            dr["StringValue1"] = "BOOM";
            dr["DateTimeValue1"] = testDt;
            dr["DateTimeValueNullable"] = DBNull.Value;
            dr["FloatValue"] = 3.5F;
            dr["GuidValue"] = testGuid;
            dr["DecimalValue"] = 9.1m;
            dr["CharValue"] = 'c';

            dt.Rows.Add(dr);

            var helper = new SqlDataAccessHelper();
            var oHolder = SqlMapper.MapSingle<MappingTestObject>(dt, helper.ExecutionContext);

            Assert.AreEqual(69, oHolder.IntegerValue1);
            Assert.AreEqual(null, oHolder.IntegerValueNullable);
            Assert.AreEqual("BOOM", oHolder.StringValue1);
            Assert.AreEqual(testDt.ToString(), oHolder.DateTimeValue1.ToString());
            Assert.AreEqual(null, oHolder.DateTimeValueNullable);
            Assert.AreEqual(3.5F, oHolder.FloatValue);
            Assert.AreEqual(testGuid, oHolder.GuidValue);
            Assert.AreEqual(9.1m, oHolder.DecimalValue);
            Assert.AreEqual('c', oHolder.CharValue);
        }

        [TestMethod]
        public void TestObjectMappingSingle()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("StringValue1", typeof(string))
            });

            var dr = dt.NewRow();
            dr["IntegerValue1"] = 1;
            dr["StringValue1"] = "BOOM";
            dt.Rows.Add(dr);

            var testO = SqlMapper.MapSingle<MappingTestObject>(dt, new ExecutionContext()
            {
                CustomColumnMappings = new Dictionary<string, string>(),
                TableSchema = "dbo"
            });

            Assert.IsTrue(testO.IntegerValue1 == 1);
            Assert.IsTrue(testO.StringValue1 == "BOOM");
        }

        [TestMethod]
        public void TestObjectMappingCollection()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("StringValue1", typeof(string))
            });

            var i = 0;
            while (i < 10)
            {
                var dr = dt.NewRow();
                dr["IntegerValue1"] = i;
                dr["StringValue1"] = "BOOM" + i;
                dt.Rows.Add(dr);
                i++;
            }

            var testCollection = SqlMapper.Map<MappingTestObject>(dt, new ExecutionContext()
            {
                CustomColumnMappings = new Dictionary<string, string>(),
                TableSchema = "dbo"
            });

            Assert.IsTrue(testCollection.Count == 10);

            i = 0;
            foreach (var vmto in testCollection)
            {
                Assert.IsTrue(vmto.IntegerValue1 == i);
                Assert.IsTrue(vmto.StringValue1 == "BOOM" + i);

                i++;
            }
        }

        [TestMethod]
        public void TestObjectKeyedCollection()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("StringValue1", typeof(string))
            });

            var i = 0;
            while (i < 10)
            {
                var dr = dt.NewRow();
                dr["IntegerValue1"] = i;
                dr["StringValue1"] = "BOOM" + i;
                dt.Rows.Add(dr);
                i++;
            }

            var ee = new ExecutionContext()
            {
                CustomColumnMappings = new Dictionary<string, string>(),
                TableSchema = "dbo"
            };

            var testCollection1 = SqlMapper.Map<MappingTestObject>(dt, ee);

            var testCollection2 =
                SqlMapper.KeyedMap<int, MappingTestObject>("IntegerValue1", testCollection1, ee, false);

            Assert.IsTrue(testCollection2.Keys.Count == 10);
        }

        [TestMethod]
        public void TestCustomColumnMappings()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("customintcolumn", typeof(int)),
                new DataColumn("customstringcolumn", typeof(string))
            });

            var dr = dt.NewRow();
            dr["customintcolumn"] = 1;
            dr["customstringcolumn"] = "BOOM";
            dt.Rows.Add(dr);

            var testO = SqlMapper.MapSingle<MappingTestObject>(dt, new ExecutionContext()
            {
                CustomColumnMappings = new Dictionary<string, string>() { { "customintcolumn", "IntegerValue1" }, { "customstringcolumn", "StringValue1" } },
                TableSchema = "dbo"
            });

            Assert.IsTrue(testO.IntegerValue1 == 1);
            Assert.IsTrue(testO.StringValue1 == "BOOM");
        }

        [TestMethod]
        public void TestCustomValueFormatterInt()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("StringValue1", typeof(string))
            });

            var dr = dt.NewRow();
            dr["IntegerValue1"] = 1;
            dr["StringValue1"] = "BOOM";
            dt.Rows.Add(dr);

            var helper = new SqlDataAccessHelper();
            helper.SetPostProcessFunction<int>("IntegerValue1", x =>
            {
                var initial = int.Parse(x);

                return initial + 1;
            });


            var testO = SqlMapper.MapSingle<MappingTestObject>(dt, helper.ExecutionContext);

            Assert.IsTrue(testO.IntegerValue1 == 2);
        }

        [TestMethod]
        public void TestCustomValueFormatterString()
        {
            var dt = new DataTable();
            dt.Columns.AddRange(new[]
            {
                new DataColumn("IntegerValue1", typeof(int)),
                new DataColumn("StringValue1", typeof(string))
            });

            var dr = dt.NewRow();
            dr["IntegerValue1"] = 1;
            dr["StringValue1"] = "BOOM";
            dt.Rows.Add(dr);

            var helper = new SqlDataAccessHelper();
            helper.SetPostProcessFunction<string>("StringValue1", x => $"{x}-FORMATTED");


            var testO = SqlMapper.MapSingle<MappingTestObject>(dt, helper.ExecutionContext);

            Assert.IsTrue(testO.StringValue1 == "BOOM-FORMATTED");
            Assert.IsTrue(testO.IntegerValue1 == 1);
        }
    }

    public class MappingTestObject
    {
        public int IntegerValue1 { get; set; }
        public int? IntegerValueNullable { get; set; }
        public string StringValue1 { get; set; }
        public DateTime DateTimeValue1 { get; set; }
        public DateTime? DateTimeValueNullable { get; set; }
        public float FloatValue { get; set; }
        public Guid GuidValue { get; set; }
        public decimal DecimalValue { get; set; }
        public char CharValue { get; set; }
    }
}
