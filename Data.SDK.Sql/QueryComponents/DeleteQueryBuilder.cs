using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class DeleteQueryBuilder : IQueryBuilder
    {
        private readonly WhereStatement whereStatement = new WhereStatement();
        private DbProviderFactory dbProviderFactory;

        public DeleteQueryBuilder()
        {
            EnableClear = false;
            Table = "";
        }

        public DeleteQueryBuilder(string table)
        {
            EnableClear = false;
            Table = table;
        }

        public DeleteQueryBuilder(DbProviderFactory factory)
        {
            EnableClear = false;
            Table = "";
            dbProviderFactory = factory;
        }

        public DeleteQueryBuilder(string table, DbProviderFactory factory)
        {
            EnableClear = false;
            Table = table;
            dbProviderFactory = factory;
        }

        public bool EnableClear { get; set; }

        public string Table { get; set; }

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

        public void AddWhere(WhereClause clause, int level = 1)
        {
            whereStatement.Add(clause, level);
        }

        public WhereClause AddWhere(Enum field, Comparison @operator, object compareValue)
        {
            return AddWhere(field.ToString(), @operator, compareValue);
        }

        public WhereClause AddWhere(string field, Comparison @operator, object compareValue, int level = 1)
        {
            var clause = new WhereClause(field, @operator, compareValue);
            whereStatement.Add(clause, level);
            return clause;
        }

        private static string AddBrakets(string originalValue)
        {
            if (originalValue.StartsWith("[")) return originalValue;

            return "[" + originalValue.Replace(".", "].[") + "]";
        }

        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");
            DbCommand usedDbCommand = null;
            if (buildCommand)
                usedDbCommand = dbProviderFactory.CreateCommand();
            const string starter = "DELETE FROM ";
            if (Table.Length == 0)
                throw new Exception("Table to delete from was not set.");
            var starterCleaned = starter + AddBrakets(Table);
            if (whereStatement.ClauseLevels > 0)
                starterCleaned = starterCleaned + " WHERE " +
                                 whereStatement.BuildWhereStatement(buildCommand, ref usedDbCommand);
            else if (!EnableClear)
                throw new Exception("Statement would delete all records in table" + Table +
                                    ". If this is want you want, set the EnableClear property to true.");
            if (!buildCommand)
                return starterCleaned;

            // ReSharper disable once PossibleNullReferenceException
            usedDbCommand.CommandText = starterCleaned;

            return usedDbCommand;
        }
    }
}