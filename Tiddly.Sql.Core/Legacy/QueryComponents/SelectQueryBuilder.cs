﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Tiddly.Sql.Legacy.QueryComponents.Clauses;
using Tiddly.Sql.Legacy.QueryComponents.Components;
using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents
{
    public class SelectQueryBuilder : IQueryBuilder
    {
        private readonly bool _distinct = false;
        private readonly List<string> _groupByColumns = new List<string>();
        private readonly List<JoinClause> _joins = new List<JoinClause>();
        private readonly List<OrderByClause> _orderByStatement = new List<OrderByClause>();
        private readonly List<string> _selectedColumns = new List<string>();
        private readonly List<string> _selectedTables = new List<string>();
        private DbProviderFactory _dbProviderFactory;
        private WhereStatement _havingStatement = new WhereStatement();
        private TopClause _topClause = new TopClause(100, TopUnit.Percent);
        private WhereStatement _whereStatement = new WhereStatement();

        public SelectQueryBuilder()
        {
            _dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        }

        public int TopRecords
        {
            get { return _topClause.Quantity; }
            set
            {
                _topClause.Quantity = value;
                _topClause.Unit = TopUnit.Records;
            }
        }

        public TopClause TopClause
        {
            get { return _topClause; }
            set { _topClause = value; }
        }

        public string[] SelectedColumns
        {
            get
            {
                return _selectedColumns.Count > 0
                    ? _selectedColumns.ToArray()
                    : new[] { "*" };
            }
        }

        public string[] SelectedTables
        {
            get { return _selectedTables.ToArray(); }
        }

        public WhereStatement Where
        {
            get { return _whereStatement; }
            set { _whereStatement = value; }
        }

        public WhereStatement Having
        {
            get { return _havingStatement; }
            set { _havingStatement = value; }
        }

        public void SetDbProviderFactory(DbProviderFactory factory)
        {
            _dbProviderFactory = factory;
        }

        public DbCommand BuildCommand()
        {
            return (DbCommand)BuildQuery(true);
        }

        public string BuildQuery()
        {
            return (string)BuildQuery(false);
        }

        public DbCommand BuildCommand(DbProviderFactory factory)
        {
            SetDbProviderFactory(factory);
            return (DbCommand)BuildQuery(true);
        }

        public void SelectAllColumns()
        {
            _selectedColumns.Clear();
        }

        public void SelectCount()
        {
            SelectColumn("count(1)");
        }

        public void SelectColumn(string column)
        {
            _selectedColumns.Clear();
            _selectedColumns.Add(column);
        }

        public void SelectColumns(params string[] columns)
        {
            _selectedColumns.Clear();
            foreach (var column in columns)
            {
                _selectedColumns.Add(column);
            }
        }

        public void SelectFromTable(string table)
        {
            _selectedTables.Clear();
            _selectedTables.Add(table);
        }

        public void SelectFromTables(params string[] tables)
        {
            _selectedTables.Clear();
            foreach (var Table in tables)
            {
                _selectedTables.Add(Table);
            }
        }

        public void AddJoin(JoinClause newJoin)
        {
            _joins.Add(newJoin);
        }

        public void AddJoin(JoinType join, string toTableName, string toColumnName, Comparison @operator,
            string fromTableName, string fromColumnName)
        {
            var NewJoin = new JoinClause(join, toTableName, toColumnName, @operator, fromTableName, fromColumnName);
            _joins.Add(NewJoin);
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
            var NewWhereClause = new WhereClause(field, @operator, compareValue);
            _whereStatement.Add(NewWhereClause, level);
            return NewWhereClause;
        }

        public void AddOrderBy(OrderByClause clause)
        {
            _orderByStatement.Add(clause);
        }

        public void AddOrderBy(Enum field, Sorting order)
        {
            AddOrderBy(field.ToString(), order);
        }

        public void AddOrderBy(string field, Sorting order)
        {
            var NewOrderByClause = new OrderByClause(field, order);
            _orderByStatement.Add(NewOrderByClause);
        }

        public void GroupBy(params string[] columns)
        {
            foreach (var Column in columns)
            {
                _groupByColumns.Add(Column);
            }
        }

        public void AddHaving(WhereClause clause)
        {
            AddHaving(clause, 1);
        }

        public void AddHaving(WhereClause clause, int level)
        {
            _havingStatement.Add(clause, level);
        }

        public WhereClause AddHaving(string field, Comparison @operator, object compareValue)
        {
            return AddHaving(field, @operator, compareValue, 1);
        }

        public WhereClause AddHaving(Enum field, Comparison @operator, object compareValue)
        {
            return AddHaving(field.ToString(), @operator, compareValue, 1);
        }

        public WhereClause AddHaving(string field, Comparison @operator, object compareValue, int level)
        {
            var NewWhereClause = new WhereClause(field, @operator, compareValue);
            _havingStatement.Add(NewWhereClause, level);
            return NewWhereClause;
        }

        /// <summary>
        ///     Builds the select query
        /// </summary>
        /// <returns>Returns a string containing the query, or a DbCommand containing a command with parameters</returns>
        private object BuildQuery(bool buildCommand)
        {
            if (buildCommand && _dbProviderFactory == null)
                throw new Exception(
                    "Cannot build a command when the Db Factory hasn't been specified. Call SetDbProviderFactory first.");

            DbCommand command = null;
            if (buildCommand)
                command = _dbProviderFactory.CreateCommand();

            var Query = "SELECT ";

            // Output Distinct
            if (_distinct)
            {
                Query += "DISTINCT ";
            }

            // Output Top clause
            if (!(_topClause.Quantity == 100 & _topClause.Unit == TopUnit.Percent))
            {
                Query += "TOP " + _topClause.Quantity;
                if (_topClause.Unit == TopUnit.Percent)
                {
                    Query += " PERCENT";
                }
                Query += " ";
            }

            // Output column names
            if (_selectedColumns.Count == 0)
            {
                if (_selectedTables.Count == 1)
                    Query += _selectedTables[0] + ".";
                        // By default only select * from the table that was selected. If there are any joins, it is the responsibility of the user to select the needed columns.

                Query += "*";
            }
            else
            {
                foreach (var ColumnName in _selectedColumns)
                {
                    Query += ColumnName + ',';
                }
                Query = Query.TrimEnd(','); // Trim de last comma inserted by foreach loop
                Query += ' ';
            }
            // Output table names
            if (_selectedTables.Count > 0)
            {
                Query += " FROM ";
                foreach (var TableName in _selectedTables)
                {
                    Query += TableName + ',';
                }
                Query = Query.TrimEnd(','); // Trim de last comma inserted by foreach loop
                Query += ' ';
            }

            // Output joins
            if (_joins.Count > 0)
            {
                foreach (var Clause in _joins)
                {
                    string JoinString;
                    switch (Clause.JoinType)
                    {
                        case JoinType.InnerJoin:
                            JoinString = "INNER JOIN";
                            break;
                        case JoinType.OuterJoin:
                            JoinString = "OUTER JOIN";
                            break;
                        case JoinType.LeftJoin:
                            JoinString = "LEFT JOIN";
                            break;
                        case JoinType.RightJoin:
                            JoinString = "RIGHT JOIN";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    JoinString += " " + Clause.ToTable + " ON ";
                    JoinString += WhereStatement.CreateComparisonClause(Clause.FromTable + '.' + Clause.FromColumn,
                        Clause.ComparisonOperator, new SqlLiteral(Clause.ToTable + '.' + Clause.ToColumn));
                    Query += JoinString + ' ';
                }
            }

            // Output where statement
            if (_whereStatement.ClauseLevels > 0)
            {
                if (buildCommand)
                    Query += " WHERE " + _whereStatement.BuildWhereStatement(true, ref command);
                else
                    Query += " WHERE " + _whereStatement.BuildWhereStatement();
            }

            // Output GroupBy statement
            if (_groupByColumns.Count > 0)
            {
                Query += " GROUP BY ";
                foreach (var Column in _groupByColumns)
                {
                    Query += Column + ',';
                }
                Query = Query.TrimEnd(',');
                Query += ' ';
            }

            // Output having statement
            if (_havingStatement.ClauseLevels > 0)
            {
                // Check if a Group By Clause was set
                if (_groupByColumns.Count == 0)
                {
                    throw new Exception("Having statement was set without Group By");
                }
                if (buildCommand)
                    Query += " HAVING " + _havingStatement.BuildWhereStatement(true, ref command);
                else
                    Query += " HAVING " + _havingStatement.BuildWhereStatement();
            }

            // Output OrderBy statement
            if (_orderByStatement.Count > 0)
            {
                Query += " ORDER BY ";
                foreach (var Clause in _orderByStatement)
                {
                    string OrderByClause;
                    switch (Clause.SortOrder)
                    {
                        case Sorting.Ascending:
                            OrderByClause = Clause.FieldName + " ASC";
                            break;

                        case Sorting.Descending:
                            OrderByClause = Clause.FieldName + " DESC";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Query += OrderByClause + ',';
                }
                Query = Query.TrimEnd(','); // Trim de last AND inserted by foreach loop
                Query += ' ';
            }

            if (!buildCommand) return Query;

            // Return the build command
            Debug.Assert(command != null, "command != null");

            command.CommandText = Query;
            return command;
            // Return the built query
        }
    }
}