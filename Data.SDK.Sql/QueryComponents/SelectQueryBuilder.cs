using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Components;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class SelectQueryBuilder : IQueryBuilder
    {
        //private const bool Distinct = false;
        private readonly List<string> groupByColumns = new List<string>();
        private readonly List<JoinClause> joins = new List<JoinClause>();
        private readonly List<OrderByClause> orderByStatement = new List<OrderByClause>();
        private readonly List<string> selectedColumns = new List<string>();
        private readonly List<string> selectedTables = new List<string>();
        private DbProviderFactory dbProviderFactory;
        private TopClause topClause = new TopClause(100, TopUnit.Percent);

        public SelectQueryBuilder()
        {
            dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        public WhereStatement Having { get; set; } = new WhereStatement();

        public string[] SelectedColumns => selectedColumns.Count > 0
            ? selectedColumns.ToArray()
            : new[] {"*"};

        public string[] SelectedTables => selectedTables.ToArray();

        public TopClause TopClause
        {
            get => topClause;
            set => topClause = value;
        }

        public int TopRecords
        {
            get => topClause.Quantity;
            set
            {
                topClause.Quantity = value;
                topClause.Unit = TopUnit.Records;
            }
        }

        public WhereStatement Where { get; set; } = new WhereStatement();

        public void SetDbProviderFactory(DbProviderFactory factory)
        {
            dbProviderFactory = factory;
        }

        public DbCommand BuildCommand()
        {
            return (DbCommand) BuildQuery(true);
        }

        public string BuildQuery()
        {
            return (string) BuildQuery(false);
        }

        public DbCommand BuildCommand(DbProviderFactory factory)
        {
            SetDbProviderFactory(factory);
            return (DbCommand) BuildQuery(true);
        }

        public void AddHaving(WhereClause clause, int level = 1)
        {
            Having.Add(clause, level);
        }

        public WhereClause AddHaving(Enum field, Comparison @operator, object compareValue)
        {
            return AddHaving(field.ToString(), @operator, compareValue);
        }

        public WhereClause AddHaving(string field, Comparison @operator, object compareValue, int level = 1)
        {
            var newWhereClause = new WhereClause(field, @operator, compareValue);
            Having.Add(newWhereClause, level);
            return newWhereClause;
        }

        public void AddJoin(JoinClause newJoin)
        {
            joins.Add(newJoin);
        }

        public void AddJoin(JoinType join, string toTableName, string toColumnName, Comparison @operator,
            string fromTableName, string fromColumnName)
        {
            var newJoin = new JoinClause(join, toTableName, toColumnName, @operator, fromTableName, fromColumnName);
            joins.Add(newJoin);
        }

        public void AddOrderBy(OrderByClause clause)
        {
            orderByStatement.Add(clause);
        }

        public void AddOrderBy(Enum field, Sorting order)
        {
            AddOrderBy(field.ToString(), order);
        }

        public void AddOrderBy(string field, Sorting order)
        {
            var newOrderByClause = new OrderByClause(field, order);
            orderByStatement.Add(newOrderByClause);
        }

        public void AddWhere(WhereClause clause, int level = 1)
        {
            Where.Add(clause, level);
        }

        public WhereClause AddWhere(Enum field, Comparison @operator, object compareValue)
        {
            return AddWhere(field.ToString(), @operator, compareValue);
        }

        public WhereClause AddWhere(string field, Comparison @operator, object compareValue, int level = 1)
        {
            var newWhereClause = new WhereClause(field, @operator, compareValue);
            Where.Add(newWhereClause, level);
            return newWhereClause;
        }

        public void GroupBy(params string[] columns)
        {
            foreach (var column in columns)
                groupByColumns.Add(column);
        }

        public void SelectAllColumns()
        {
            selectedColumns.Clear();
        }

        public void SelectColumn(string column)
        {
            selectedColumns.Clear();
            selectedColumns.Add(column);
        }

        public void SelectColumns(params string[] columns)
        {
            selectedColumns.Clear();
            foreach (var column in columns)
                selectedColumns.Add(column);
        }

        public void SelectCount()
        {
            SelectColumn("count(1)");
        }

        public void SelectFromTable(string table)
        {
            selectedTables.Clear();
            selectedTables.Add(table);
        }

        public void SelectFromTables(params string[] tables)
        {
            selectedTables.Clear();
            foreach (var table in tables)
                selectedTables.Add(table);
        }

        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");

            DbCommand command = null;
            if (buildCommand)
                command = dbProviderFactory.CreateCommand();

            var query = "SELECT ";

/*
            if (Distinct)
                query += "DISTINCT ";
*/

            if (!((topClause.Quantity == 100) & (topClause.Unit == TopUnit.Percent)))
            {
                query += "TOP " + topClause.Quantity;
                if (topClause.Unit == TopUnit.Percent)
                    query += " PERCENT";
                query += " ";
            }

            if (selectedColumns.Count == 0)
            {
                if (selectedTables.Count == 1)
                    query += selectedTables[0] + ".";

                query += "*";
            }
            else
            {
                foreach (var columnName in selectedColumns)
                    query += columnName + ',';
                query = query.TrimEnd(',');
                query += ' ';
            }

            if (selectedTables.Count > 0)
            {
                query += " FROM ";
                foreach (var tableName in selectedTables)
                    query += tableName + ',';
                query = query.TrimEnd(',');
                query += ' ';
            }

            if (joins.Count > 0)
                foreach (var clause in joins)
                {
                    string joinString;
                    switch (clause.JoinType)
                    {
                        case JoinType.InnerJoin:
                            joinString = "INNER JOIN";
                            break;
                        case JoinType.OuterJoin:
                            joinString = "OUTER JOIN";
                            break;
                        case JoinType.LeftJoin:
                            joinString = "LEFT JOIN";
                            break;
                        case JoinType.RightJoin:
                            joinString = "RIGHT JOIN";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    joinString += " " + clause.ToTable + " ON ";
                    joinString += WhereStatement.CreateComparisonClause(clause.FromTable + '.' + clause.FromColumn,
                        clause.ComparisonOperator, new SqlLiteral(clause.ToTable + '.' + clause.ToColumn));
                    query += joinString + ' ';
                }

            if (Where.ClauseLevels > 0)
                if (buildCommand)
                    query += " WHERE " + Where.BuildWhereStatement(true, ref command);
                else
                    query += " WHERE " + Where.BuildWhereStatement();

            if (groupByColumns.Count > 0)
            {
                query += " GROUP BY ";
                foreach (var column in groupByColumns)
                    query += column + ',';
                query = query.TrimEnd(',');
                query += ' ';
            }

            if (Having.ClauseLevels > 0)
            {
                if (groupByColumns.Count == 0)
                    throw new Exception("Having statement was set without Group By");
                if (buildCommand)
                    query += " HAVING " + Having.BuildWhereStatement(true, ref command);
                else
                    query += " HAVING " + Having.BuildWhereStatement();
            }

            if (orderByStatement.Count > 0)
            {
                query += " ORDER BY ";
                foreach (var clause in orderByStatement)
                {
                    string orderByClause;
                    switch (clause.SortOrder)
                    {
                        case Sorting.Ascending:
                            orderByClause = clause.FieldName + " ASC";
                            break;

                        case Sorting.Descending:
                            orderByClause = clause.FieldName + " DESC";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    query += orderByClause + ',';
                }

                query = query.TrimEnd(',');
                query += ' ';
            }

            if (!buildCommand) return query;

            // ReSharper disable once PossibleNullReferenceException
            command.CommandText = query;

            return command;
        }
    }
}