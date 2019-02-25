using System;
using System.Data.Common;
using System.Diagnostics;
using Tiddly.Sql.Legacy.QueryComponents.Clauses;
using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents
{
    public class DeleteQueryBuilder : IQueryBuilder
    {
        private readonly WhereStatement _whereStatement = new WhereStatement();
        private DbProviderFactory _dbProviderFactory;

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
            _dbProviderFactory = factory;
        }

        public DeleteQueryBuilder(string table, DbProviderFactory factory)
        {
            EnableClear = false;
            Table = table;
            _dbProviderFactory = factory;
        }

        public string Table { get; set; }

        public bool EnableClear { get; set; }

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
            DbCommand usedDbCommand = null;
            if (buildCommand)
                usedDbCommand = _dbProviderFactory.CreateCommand();
            const string starter = "DELETE FROM ";
            if (Table.Length == 0)
                throw new Exception("Table to delete from was not set.");
            var starterCleaned = starter + AddBrakets(Table);
            if (_whereStatement.ClauseLevels > 0)
                starterCleaned = starterCleaned + " WHERE " +
                                 _whereStatement.BuildWhereStatement(buildCommand, ref usedDbCommand);
            else if (!EnableClear)
                throw new Exception("Statement would delete all records in table" + Table +
                                    ". If this is want you want, set the EnableClear property to true.");
            if (!buildCommand)
                return starterCleaned;

            Debug.Assert(usedDbCommand != null, "usedDbCommand != null");

            usedDbCommand.CommandText = starterCleaned;

            return usedDbCommand;
        }

        private static string AddBrakets(string originalValue)
        {
            if (originalValue.StartsWith("[")) return originalValue;

            return "[" + originalValue.Replace(".", "].[") + "]";
        }
    }
}