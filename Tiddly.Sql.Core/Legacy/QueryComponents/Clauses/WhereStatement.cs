using System;
using System.Collections.Generic;
using System.Data.Common;
using Tiddly.Sql.Legacy.QueryComponents.Components;
using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents.Clauses
{
    public class WhereStatement : List<List<WhereClause>>
    {
        public int ClauseLevels
        {
            get { return Count; }
        }

        private void AssertLevelExistance(int level)
        {
            if (Count < level - 1)
            {
                throw new Exception("Level " + level + " not allowed because level " + (level - 1) + " does not exist.");
            }
            if (Count < level)
            {
                Add(new List<WhereClause>());
            }
        }

        public void Add(WhereClause clause)
        {
            Add(clause, 1);
        }

        public void Add(WhereClause clause, int level)
        {
            AddWhereClauseToLevel(clause, level);
        }

        public WhereClause Add(string field, Comparison @operator, object compareValue)
        {
            return Add(field, @operator, compareValue, 1);
        }

        public WhereClause Add(Enum field, Comparison @operator, object compareValue)
        {
            return Add(field.ToString(), @operator, compareValue, 1);
        }

        public WhereClause Add(string field, Comparison @operator, object compareValue, int level)
        {
            var NewWhereClause = new WhereClause(field, @operator, compareValue);
            AddWhereClauseToLevel(NewWhereClause, level);
            return NewWhereClause;
        }

        private void AddWhereClauseToLevel(WhereClause clause, int level)
        {
            // Add the new clause to the array at the right level
            AssertLevelExistance(level);
            this[level - 1].Add(clause);
        }

        public string BuildWhereStatement()
        {
            DbCommand dummyCommand = null; // = DataAccess.UsedDbProviderFactory.CreateCommand();
            return BuildWhereStatement(false, ref dummyCommand);
        }

        public string BuildWhereStatement(bool useCommandObject, ref DbCommand usedDbCommand)
        {
            var Result = "";
            foreach (var WhereStatement in this) // Loop through all statement levels, OR them together
            {
                var LevelWhere = "";
                foreach (var Clause in WhereStatement) // Loop through all conditions, AND them together
                {
                    var WhereClause = "";

                    if (useCommandObject)
                    {
                        // Create a parameter
                        var parameterName = string.Format(
                            "@p{0}_{1}",
                            usedDbCommand.Parameters.Count + 1,
                            Clause.FieldName.Replace('.', '_')
                            );

                        var parameter = usedDbCommand.CreateParameter();
                        parameter.ParameterName = parameterName;
                        parameter.Value = Clause.Value;
                        usedDbCommand.Parameters.Add(parameter);

                        // Create a where clause using the parameter, instead of its value
                        WhereClause += CreateComparisonClause(Clause.FieldName, Clause.ComparisonOperator,
                            new SqlLiteral(parameterName));
                    }
                    else
                    {
                        WhereClause = CreateComparisonClause(Clause.FieldName, Clause.ComparisonOperator, Clause.Value);
                    }

                    foreach (var SubWhereClause in Clause.SubClauses)
                        // Loop through all subclauses, append them together with the specified logic operator
                    {
                        switch (SubWhereClause.LogicOperator)
                        {
                            case LogicOperator.And:
                                WhereClause += " AND ";
                                break;
                            case LogicOperator.Or:
                                WhereClause += " OR ";
                                break;
                        }

                        if (useCommandObject)
                        {
                            // Create a parameter
                            var parameterName = string.Format(
                                "@p{0}_{1}",
                                usedDbCommand.Parameters.Count + 1,
                                Clause.FieldName.Replace('.', '_')
                                );

                            var parameter = usedDbCommand.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = SubWhereClause.Value;
                            usedDbCommand.Parameters.Add(parameter);

                            // Create a where clause using the parameter, instead of its value
                            WhereClause += CreateComparisonClause(Clause.FieldName, SubWhereClause.ComparisonOperator,
                                new SqlLiteral(parameterName));
                        }
                        else
                        {
                            WhereClause += CreateComparisonClause(Clause.FieldName, SubWhereClause.ComparisonOperator,
                                SubWhereClause.Value);
                        }
                    }
                    LevelWhere += "(" + WhereClause + ") AND ";
                }
                LevelWhere = LevelWhere.Substring(0, LevelWhere.Length - 5);
                    // Trim de last AND inserted by foreach loop
                if (WhereStatement.Count > 1)
                {
                    Result += " (" + LevelWhere + ") ";
                }
                else
                {
                    Result += " " + LevelWhere + " ";
                }
                Result += " OR";
            }
            Result = Result.Substring(0, Result.Length - 2); // Trim de last OR inserted by foreach loop

            return Result;
        }

        internal static string CreateComparisonClause(string fieldName, Comparison comparisonOperator, object value)
        {
            var Output = "";
            if (value != null && value != DBNull.Value)
            {
                switch (comparisonOperator)
                {
                    case Comparison.Equals:
                        Output = fieldName + " = " + FormatSQLValue(value);
                        break;
                    case Comparison.NotEquals:
                        Output = fieldName + " <> " + FormatSQLValue(value);
                        break;
                    case Comparison.GreaterThan:
                        Output = fieldName + " > " + FormatSQLValue(value);
                        break;
                    case Comparison.GreaterOrEquals:
                        Output = fieldName + " >= " + FormatSQLValue(value);
                        break;
                    case Comparison.LessThan:
                        Output = fieldName + " < " + FormatSQLValue(value);
                        break;
                    case Comparison.LessOrEquals:
                        Output = fieldName + " <= " + FormatSQLValue(value);
                        break;
                    case Comparison.Like:
                        Output = fieldName + " LIKE " + FormatSQLValue(value);
                        break;
                    case Comparison.NotLike:
                        Output = "NOT " + fieldName + " LIKE " + FormatSQLValue(value);
                        break;
                    case Comparison.In:
                        Output = fieldName + " IN (" + FormatSQLValue(value) + ")";
                        break;
                }
            }
            else // value==null	|| value==DBNull.Value
            {
                if ((comparisonOperator != Comparison.Equals) && (comparisonOperator != Comparison.NotEquals))
                {
                    throw new Exception("Cannot use comparison operator " + comparisonOperator + " for NULL values.");
                }

                switch (comparisonOperator)
                {
                    case Comparison.Equals:
                        Output = fieldName + " IS NULL";
                        break;
                    case Comparison.NotEquals:
                        Output = "NOT " + fieldName + " IS NULL";
                        break;
                }
            }
            return Output;
        }

        internal static string FormatSQLValue(object someValue)
        {
            string FormattedValue;
            //				string StringType = Type.GetType("string").Name;
            //				string DateTimeType = Type.GetType("DateTime").Name;

            if (someValue == null)
            {
                FormattedValue = "NULL";
            }
            else
            {
                switch (someValue.GetType().Name)
                {
                    case "String":
                        FormattedValue = "'" + ((string) someValue).Replace("'", "''") + "'";
                        break;
                    case "DateTime":
                        FormattedValue = "'" + ((DateTime) someValue).ToString("yyyy/MM/dd hh:mm:ss") + "'";
                        break;
                    case "DBNull":
                        FormattedValue = "NULL";
                        break;
                    case "Boolean":
                        FormattedValue = (bool) someValue ? "1" : "0";
                        break;
                    case "SqlLiteral":
                        FormattedValue = ((SqlLiteral) someValue).Value;
                        break;
                    default:
                        FormattedValue = someValue.ToString();
                        break;
                }
            }
            return FormattedValue;
        }

        /// <summary>
        ///     This static method combines 2 where statements with eachother to form a new statement
        /// </summary>
        /// <param name="statement1"></param>
        /// <param name="statement2"></param>
        /// <returns></returns>
        public static WhereStatement CombineStatements(WhereStatement statement1, WhereStatement statement2)
        {
            // statement1: {Level1}((Age<15 OR Age>=20) AND (strEmail LIKE 'e%') OR {Level2}(Age BETWEEN 15 AND 20))
            // Statement2: {Level1}((Name = 'Peter'))
            // Return statement: {Level1}((Age<15 or Age>=20) AND (strEmail like 'e%') AND (Name = 'Peter'))

            // Make a copy of statement1
            var result = Copy(statement1);

            // Add all clauses of statement2 to result
            for (var i = 0; i < statement2.ClauseLevels; i++) // for each clause level in statement2
            {
                var level = statement2[i];
                foreach (var clause in level) // for each clause in level i
                {
                    for (var j = 0; j < result.ClauseLevels; j++) // for each level in result, add the clause
                    {
                        result.AddWhereClauseToLevel(clause, j);
                    }
                }
            }

            return result;
        }

        public static WhereStatement Copy(WhereStatement statement)
        {
            var result = new WhereStatement();
            var currentLevel = 0;
            // ReSharper disable once UnusedVariable
            foreach (var level in statement)
            {
                currentLevel++;
                result.Add(new List<WhereClause>());
                foreach (var clause in statement[currentLevel - 1])
                {
                    var clauseCopy = new WhereClause(clause.FieldName, clause.ComparisonOperator, clause.Value);
                    foreach (var subClause in clause.SubClauses)
                    {
                        var subClauseCopy = new WhereClause.SubClause(subClause.LogicOperator,
                            subClause.ComparisonOperator, subClause.Value);
                        clauseCopy.SubClauses.Add(subClauseCopy);
                    }
                    result[currentLevel - 1].Add(clauseCopy);
                }
            }
            return result;
        }
    }
}