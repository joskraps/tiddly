using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tiddly.Sql.QueryComponents.Components;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents.Clauses
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class WhereStatement : List<List<WhereClause>>
    {
        public int ClauseLevels => Count;

        public void Add(WhereClause clause, int level = 1)
        {
            AddWhereClauseToLevel(clause, level);
        }

        public WhereClause Add(Enum field, Comparison @operator, object compareValue)
        {
            return Add(field.ToString(), @operator, compareValue);
        }

        public WhereClause Add(string field, Comparison @operator, object compareValue, int level = 1)
        {
            var newWhereClause = new WhereClause(field, @operator, compareValue);
            AddWhereClauseToLevel(newWhereClause, level);
            return newWhereClause;
        }

        public string BuildWhereStatement()
        {
            DbCommand dummyCommand = null; // = DataAccess.UsedDbProviderFactory.CreateCommand();
            return BuildWhereStatement(false, ref dummyCommand);
        }

        public string BuildWhereStatement(bool useCommandObject, ref DbCommand usedDbCommand)
        {
            var result = "";
            foreach (var whereStatement in this) // Loop through all statement levels, OR them together
            {
                var levelWhere = "";
                foreach (var clause in whereStatement) // Loop through all conditions, AND them together
                {
                    var whereClause = "";

                    if (useCommandObject)
                    {
                        // Create a parameter
                        var parameterName =
                            $"@p{usedDbCommand.Parameters.Count + 1}_{clause.FieldName.Replace('.', '_')}";

                        var parameter = usedDbCommand.CreateParameter();
                        parameter.ParameterName = parameterName;
                        parameter.Value = clause.Value;
                        usedDbCommand.Parameters.Add(parameter);

                        // Create a where clause using the parameter, instead of its value
                        whereClause += CreateComparisonClause(clause.FieldName, clause.ComparisonOperator,
                            new SqlLiteral(parameterName));
                    }
                    else
                    {
                        whereClause = CreateComparisonClause(clause.FieldName, clause.ComparisonOperator, clause.Value);
                    }

                    foreach (var subWhereClause in clause.SubClauses)
                        // Loop through all subclauses, append them together with the specified logic operator
                    {
                        switch (subWhereClause.LogicOperator)
                        {
                            case LogicOperator.And:
                                whereClause += " AND ";
                                break;
                            case LogicOperator.Or:
                                whereClause += " OR ";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (useCommandObject)
                        {
                            // Create a parameter
                            var parameterName =
                                $"@p{usedDbCommand.Parameters.Count + 1}_{clause.FieldName.Replace('.', '_')}";

                            var parameter = usedDbCommand.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = subWhereClause.Value;
                            usedDbCommand.Parameters.Add(parameter);

                            // Create a where clause using the parameter, instead of its value
                            whereClause += CreateComparisonClause(clause.FieldName, subWhereClause.ComparisonOperator,
                                new SqlLiteral(parameterName));
                        }
                        else
                        {
                            whereClause += CreateComparisonClause(clause.FieldName, subWhereClause.ComparisonOperator,
                                subWhereClause.Value);
                        }
                    }

                    levelWhere += "(" + whereClause + ") AND ";
                }

                levelWhere = levelWhere.Substring(0, levelWhere.Length - 5);
                // Trim de last AND inserted by foreach loop
                if (whereStatement.Count > 1)
                    result += " (" + levelWhere + ") ";
                else
                    result += " " + levelWhere + " ";
                result += " OR";
            }

            result = result.Substring(0, result.Length - 2); // Trim de last OR inserted by foreach loop

            return result;
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
                    for (var j = 0; j < result.ClauseLevels; j++) // for each level in result, add the clause
                        result.AddWhereClauseToLevel(clause, j);
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

        internal static string CreateComparisonClause(string fieldName, Comparison comparisonOperator, object value)
        {
            string output;

            if (value != null && value != DBNull.Value)
            {
                switch (comparisonOperator)
                {
                    case Comparison.Equals:
                        output = fieldName + " = " + FormatSqlValue(value);
                        break;
                    case Comparison.NotEquals:
                        output = fieldName + " <> " + FormatSqlValue(value);
                        break;
                    case Comparison.GreaterThan:
                        output = fieldName + " > " + FormatSqlValue(value);
                        break;
                    case Comparison.GreaterOrEquals:
                        output = fieldName + " >= " + FormatSqlValue(value);
                        break;
                    case Comparison.LessThan:
                        output = fieldName + " < " + FormatSqlValue(value);
                        break;
                    case Comparison.LessOrEquals:
                        output = fieldName + " <= " + FormatSqlValue(value);
                        break;
                    case Comparison.Like:
                        output = fieldName + " LIKE " + FormatSqlValue(value);
                        break;
                    case Comparison.NotLike:
                        output = "NOT " + fieldName + " LIKE " + FormatSqlValue(value);
                        break;
                    case Comparison.In:
                        output = fieldName + " IN (" + FormatSqlValue(value) + ")";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(comparisonOperator), comparisonOperator, null);
                }
            }
            else // value==null	|| value==DBNull.Value
            {
                if (comparisonOperator != Comparison.Equals && comparisonOperator != Comparison.NotEquals)
                    throw new Exception("Cannot use comparison operator " + comparisonOperator + " for NULL values.");

                switch (comparisonOperator)
                {
                    case Comparison.Equals:
                        output = fieldName + " IS NULL";
                        break;
                    case Comparison.NotEquals:
                        output = "NOT " + fieldName + " IS NULL";
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case Comparison.Like:
                    case Comparison.NotLike:
                    case Comparison.GreaterThan:
                    case Comparison.GreaterOrEquals:                   
                    case Comparison.LessThan:
                    case Comparison.LessOrEquals:
                    case Comparison.In:
                    // ReSharper restore RedundantCaseLabel
                    default:
                        throw new ArgumentOutOfRangeException(nameof(comparisonOperator), comparisonOperator, null);
                }
            }

            return output;
        }

        internal static string FormatSqlValue(object someValue)
        {
            string formattedValue;
            //				string StringType = Type.GetType("string").Name;
            //				string DateTimeType = Type.GetType("DateTime").Name;

            if (someValue == null)
                formattedValue = "NULL";
            else
                switch (someValue.GetType().Name)
                {
                    case "String":
                        formattedValue = "'" + ((string) someValue).Replace("'", "''") + "'";
                        break;
                    case "DateTime":
                        formattedValue = "'" + ((DateTime) someValue).ToString("yyyy/MM/dd hh:mm:ss") + "'";
                        break;
                    case "DBNull":
                        formattedValue = "NULL";
                        break;
                    case "Boolean":
                        formattedValue = (bool) someValue ? "1" : "0";
                        break;
                    case "SqlLiteral":
                        formattedValue = ((SqlLiteral) someValue).Value;
                        break;
                    default:
                        formattedValue = someValue.ToString();
                        break;
                }
            return formattedValue;
        }

        private void AddWhereClauseToLevel(WhereClause clause, int level)
        {
            // Add the new clause to the array at the right level
            AssertLevelExistance(level);
            this[level - 1].Add(clause);
        }

        private void AssertLevelExistance(int level)
        {
            if (Count < level - 1)
                throw new Exception("Level " + level + " not allowed because level " + (level - 1) +
                                    " does not exist.");
            if (Count < level)
                Add(new List<WhereClause>());
        }
    }
}