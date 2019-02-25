using System;
using System.Collections.Generic;
using System.Data.Common;
using Tiddly.Sql.Legacy.QueryComponents.Clauses;
using Tiddly.Sql.Legacy.QueryComponents.Components;
using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents
{
    //TODO Currently does not support joins in the table
    public class UpdateQueryBuilder : IQueryBuilder
    {
        private readonly List<FieldValuePair> _fieldValuePairs = new List<FieldValuePair>();
        private readonly WhereStatement _whereStatement = new WhereStatement();
        private DbProviderFactory _dbProviderFactory;
        private string _table = "";

        public UpdateQueryBuilder()
        {
        }

        public UpdateQueryBuilder(string table)
        {
            _table = table;
        }

        public UpdateQueryBuilder(DbProviderFactory factory)
        {
            _dbProviderFactory = factory;
        }

        public UpdateQueryBuilder(string table, DbProviderFactory factory)
        {
            _table = table;
            _dbProviderFactory = factory;
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public DbCommand BuildCommand()
        {
            return (DbCommand)BuildQuery(true);
        }

        public DbCommand BuildCommand(DbProviderFactory factory)
        {
            SetDbProviderFactory(factory);
            return (DbCommand)BuildQuery(true);
        }

        public string BuildQuery()
        {
            return (string)BuildQuery(false);
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

        public void AddWhere(WhereClause clause)
        {
            AddWhere(clause, 1);
        }

        public void AddWhere(WhereClause clause, int level)
        {
            _whereStatement.Add(clause, level);
        }

        public WhereClause AddWhere(string field, Comparison @operator, object compareValue)
        {
            return AddWhere(field, @operator, compareValue, 1);
        }

        public WhereClause AddWhere(Enum field, Comparison @operator, object compareValue)
        {
            return AddWhere(field.ToString(), @operator, compareValue, 1);
        }

        public WhereClause AddWhere(string field, Comparison @operator, object compareValue, int level)
        {
            var clause = new WhereClause(field, @operator, compareValue);
            _whereStatement.Add(clause, level);
            return clause;
        }

        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && _dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");
            var usedDbCommand = (DbCommand)null;
            if (buildCommand)
                usedDbCommand = _dbProviderFactory.CreateCommand();
            var str1 = "UPDATE ";
            if (_table.Length == 0)
                throw new Exception("Table to update was not set.");
            var str2 = str1 + _table;
            if (_fieldValuePairs.Count == 0)
                throw new Exception("Nothing to update.");
            var str3 = str2 + " SET ";
            foreach (var fieldValuePair in _fieldValuePairs)
            {
                if (buildCommand && fieldValuePair.Value != null && fieldValuePair.Value != DBNull.Value &&
                    !(fieldValuePair.Value is SqlLiteral) && !(fieldValuePair.Value is SelectQueryBuilder))
                {
                    var str4 = string.Format("@p{0}_{1}", usedDbCommand.Parameters.Count + 1, fieldValuePair.FieldName);
                    var parameter = usedDbCommand.CreateParameter();
                    parameter.ParameterName = str4;
                    parameter.Value = fieldValuePair.Value;
                    usedDbCommand.Parameters.Add(parameter);
                    str3 = str3 + fieldValuePair.FieldName + "=" + str4 + ", ";
                }
                else
                {
                    str3 = str3 + fieldValuePair.FieldName + "=";
                    str3 = str3 + WhereStatement.FormatSQLValue(fieldValuePair.Value) + ", ";
                }
            }
            var str5 = str3.Substring(0, str3.Length - 2) + " ";
            if (_whereStatement.ClauseLevels > 0)
                str5 = !buildCommand
                    ? str5 + " WHERE " + _whereStatement.BuildWhereStatement()
                    : str5 + " WHERE " + _whereStatement.BuildWhereStatement(true, ref usedDbCommand);
            if (!buildCommand)
                return str5;
            usedDbCommand.CommandText = str5;
            return usedDbCommand;
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