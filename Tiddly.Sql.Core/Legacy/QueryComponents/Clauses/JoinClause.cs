using Tiddly.Sql.Legacy.QueryComponents.Enums;

namespace Tiddly.Sql.Legacy.QueryComponents.Clauses
{
    public struct JoinClause
    {
        public JoinType JoinType;
        public string FromTable;
        public string FromColumn;
        public Comparison ComparisonOperator;
        public string ToTable;
        public string ToColumn;

        public JoinClause(JoinType join, string toTableName, string toColumnName, Comparison @operator,
            string fromTableName, string fromColumnName)
        {
            JoinType = join;
            FromTable = fromTableName;
            FromColumn = fromColumnName;
            ComparisonOperator = @operator;
            ToTable = toTableName;
            ToColumn = toColumnName;
        }
    }
}