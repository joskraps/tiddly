using System;
using System.Collections.Generic;
using System.Data.Common;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Components;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents
{
    //TODO Currently does not support joins in the table
    public class UpdateQueryBuilder : IQueryBuilder
    {
        private readonly List<FieldValuePair> fieldValuePairs = new List<FieldValuePair>();
        private readonly WhereStatement whereStatement = new WhereStatement();
        private DbProviderFactory dbProviderFactory;

        public UpdateQueryBuilder()
        {
        }

        public UpdateQueryBuilder(string table)
        {
            Table = table;
        }

        public UpdateQueryBuilder(DbProviderFactory factory)
        {
            dbProviderFactory = factory;
        }

        public UpdateQueryBuilder(string table, DbProviderFactory factory)
        {
            Table = table;
            dbProviderFactory = factory;
        }

        public string Table { get; set; } = "";

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
            dbProviderFactory = factory;
        }

        public void AddWhere(WhereClause clause)
        {
            AddWhere(clause, 1);
        }

        public void AddWhere(WhereClause clause, int level)
        {
            whereStatement.Add(clause, level);
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
            whereStatement.Add(clause, level);
            return clause;
        }

        public void SetField(string fieldName, object fieldValue)
        {
            var flag = false;
            foreach (var fieldValuePair in fieldValuePairs)
                if (fieldValuePair.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldValuePair.Value = fieldValue;
                    flag = true;
                    break;
                }

            if (flag)
                return;
            fieldValuePairs.Add(new FieldValuePair(fieldName, fieldValue));
        }

        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");
            var usedDbCommand = (DbCommand) null;
            if (buildCommand)
                usedDbCommand = dbProviderFactory.CreateCommand();
            const string str1 = "UPDATE ";
            if (Table.Length == 0)
                throw new Exception("Table to update was not set.");
            var str2 = str1 + Table;
            if (fieldValuePairs.Count == 0)
                throw new Exception("Nothing to update.");
            var str3 = str2 + " SET ";
            foreach (var fieldValuePair in fieldValuePairs)
                if (buildCommand && fieldValuePair.Value != null && fieldValuePair.Value != DBNull.Value &&
                    !(fieldValuePair.Value is SqlLiteral) && !(fieldValuePair.Value is SelectQueryBuilder))
                {
                    if (usedDbCommand == null) continue;

                    var str4 = $"@p{usedDbCommand.Parameters.Count + 1}_{fieldValuePair.FieldName}";
                    var parameter = usedDbCommand.CreateParameter();
                    parameter.ParameterName = str4;
                    parameter.Value = fieldValuePair.Value;
                    usedDbCommand.Parameters.Add(parameter);
                    str3 = str3 + fieldValuePair.FieldName + "=" + str4 + ", ";
                }
                else
                {
                    str3 = str3 + fieldValuePair.FieldName + "=";
                    str3 = str3 + WhereStatement.FormatSqlValue(fieldValuePair.Value) + ", ";
                }

            var str5 = str3.Substring(0, str3.Length - 2) + " ";
            if (whereStatement.ClauseLevels > 0)
                str5 = !buildCommand
                    ? str5 + " WHERE " + whereStatement.BuildWhereStatement()
                    : str5 + " WHERE " + whereStatement.BuildWhereStatement(true, ref usedDbCommand);
            if (!buildCommand)
                return str5;

            // ReSharper disable once PossibleNullReferenceException
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