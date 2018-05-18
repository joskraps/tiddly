using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents.Clauses
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct WhereClause
    {
        internal struct SubClause
        {
            public readonly LogicOperator LogicOperator;
            public readonly Comparison ComparisonOperator;
            public readonly object Value;

            public SubClause(LogicOperator logic, Comparison compareOperator, object compareValue)
            {
                LogicOperator = logic;
                ComparisonOperator = compareOperator;
                Value = compareValue;
            }
        }

        internal readonly List<SubClause> SubClauses; // Array of SubClause

        /// <summary>
        ///     Gets/sets the name of the database column this WHERE clause should operate on
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        ///     Gets/sets the comparison method
        /// </summary>
        public Comparison ComparisonOperator { get; set; }

        /// <summary>
        ///     Gets/sets the value that was set for comparison
        /// </summary>
        public object Value { get; set; }

        public WhereClause(string field, Comparison firstCompareOperator, object firstCompareValue)
        {
            FieldName = field;
            ComparisonOperator = firstCompareOperator;
            Value = firstCompareValue;
            SubClauses = new List<SubClause>();
        }

        public void AddClause(LogicOperator logic, Comparison compareOperator, object compareValue)
        {
            var newSubClause = new SubClause(logic, compareOperator, compareValue);
            SubClauses.Add(newSubClause);
        }
    }
}