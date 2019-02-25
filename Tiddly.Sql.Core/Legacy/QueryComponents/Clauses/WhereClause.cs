using System.Collections.Generic;
using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents.Clauses
{
    public struct WhereClause
    {
        private string m_FieldName;
        private Comparison m_ComparisonOperator;
        private object m_Value;

        internal struct SubClause
        {
            public LogicOperator LogicOperator;
            public Comparison ComparisonOperator;
            public object Value;

            public SubClause(LogicOperator logic, Comparison compareOperator, object compareValue)
            {
                LogicOperator = logic;
                ComparisonOperator = compareOperator;
                Value = compareValue;
            }
        }

        internal List<SubClause> SubClauses; // Array of SubClause

        /// <summary>
        ///     Gets/sets the name of the database column this WHERE clause should operate on
        /// </summary>
        public string FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; }
        }

        /// <summary>
        ///     Gets/sets the comparison method
        /// </summary>
        public Comparison ComparisonOperator
        {
            get { return m_ComparisonOperator; }
            set { m_ComparisonOperator = value; }
        }

        /// <summary>
        ///     Gets/sets the value that was set for comparison
        /// </summary>
        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public WhereClause(string field, Comparison firstCompareOperator, object firstCompareValue)
        {
            m_FieldName = field;
            m_ComparisonOperator = firstCompareOperator;
            m_Value = firstCompareValue;
            SubClauses = new List<SubClause>();
        }

        public void AddClause(LogicOperator logic, Comparison compareOperator, object compareValue)
        {
            var NewSubClause = new SubClause(logic, compareOperator, compareValue);
            SubClauses.Add(NewSubClause);
        }
    }
}