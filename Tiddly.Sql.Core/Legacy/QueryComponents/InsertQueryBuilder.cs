using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using Tiddly.Sql.Legacy.QueryComponents.Clauses;
using Tiddly.Sql.Legacy.QueryComponents.Components;

namespace Tiddly.Sql.Legacy.QueryComponents
{
    public class InsertQueryBuilder : IQueryBuilder
    {
        private readonly List<FieldValuePair> _fieldValuePairs = new List<FieldValuePair>();
        private DbProviderFactory _dbProviderFactory;

        public InsertQueryBuilder()
        {
            Table = "";
            SetDbProviderFactory(DbProviderFactories.GetFactory("System.Data.SqlClient"));
        }

        public InsertQueryBuilder(string table)
        {
            Table = table;
            SetDbProviderFactory(DbProviderFactories.GetFactory("System.Data.SqlClient"));
        }

        public InsertQueryBuilder(DbProviderFactory factory)
        {
            Table = "";
            _dbProviderFactory = factory;
        }

        public InsertQueryBuilder(string table, DbProviderFactory factory)
        {
            Table = table;
            _dbProviderFactory = factory;
        }

        public string Table { get; set; }

        public bool SelectIdentity { get; set; }

        public DbCommand BuildCommand()
        {
            return (DbCommand) BuildQuery(true);
        }

        public DbCommand BuildCommand(DbProviderFactory factory)
        {
            SetDbProviderFactory(factory);
            return (DbCommand) BuildQuery(true);
        }

        public string BuildQuery()
        {
            return (string) BuildQuery(false);
        }

        public void SetDbProviderFactory(DbProviderFactory factory)
        {
            _dbProviderFactory = factory;
        }

        public void SetField(string fieldName, object fieldValue)
        {
            var flag = false;
            foreach (var fieldValuePair in _fieldValuePairs)
            {
                if (fieldValuePair.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldValuePair.Value = fieldValue;
                    flag = true;
                    break;
                }
            }
            if (flag)
                return;
            _fieldValuePairs.Add(new FieldValuePair(fieldName, fieldValue));
        }

        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && _dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");
            var dbCommand = (DbCommand) null;
            if (buildCommand)
                dbCommand = _dbProviderFactory.CreateCommand();
            var str1 = "INSERT INTO ";
            if (Table.Length == 0)
                throw new Exception("Table to insert to was not set.");
            var str2 = str1 + Table;
            if (_fieldValuePairs.Count == 0)
                throw new Exception("No fields set to insert.");
            var str3 = str2 + "(";
            foreach (var fieldValuePair in _fieldValuePairs)
                str3 = str3 + "" + fieldValuePair.FieldName + ", ";
            var str4 = str3.Substring(0, str3.Length - 2) + ") VALUES (";
            foreach (var fieldValuePair in _fieldValuePairs)
            {
                if (buildCommand && fieldValuePair.Value != null && fieldValuePair.Value != DBNull.Value &&
                    !(fieldValuePair.Value is SqlLiteral) && !(fieldValuePair.Value is SelectQueryBuilder))
                {
                    Debug.Assert(dbCommand != null, "dbCommand != null");

                    var str5 = string.Format("@p{0}_{1}", dbCommand.Parameters.Count + 1, fieldValuePair.FieldName);
                    var parameter = dbCommand.CreateParameter();
                    parameter.ParameterName = str5;
                    parameter.Value = fieldValuePair.Value;
                    dbCommand.Parameters.Add(parameter);
                    str4 = str4 + "" + str5 + ", ";
                }
                else
                    str4 = str4 + "" + WhereStatement.FormatSQLValue(fieldValuePair.Value) + ", ";
            }
            var str6 = str4.Substring(0, str4.Length - 2) + ")";
            if (SelectIdentity)
                str6 = str6 + "; " + BuildIdentitySelect();
            if (!buildCommand)
                return str6;

            Debug.Assert(dbCommand != null, "dbCommand != null");

            dbCommand.CommandText = str6;
            return dbCommand;
        }

        private string BuildIdentitySelect()
        {
            if (_dbProviderFactory == null)
                throw new Exception(
                    "Cannot select identity when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");
            if (_dbProviderFactory is SqlClientFactory)
                return "SELECT SCOPE_IDENTITY()";
            return "SELECT @@IDENTITY";
        }

        private class FieldValuePair
        {
            public readonly string FieldName;
            public object Value;

            public FieldValuePair(string fieldName, object fieldValue)
            {
                FieldName = fieldName;
                Value = fieldValue;
            }
        }
    }
}