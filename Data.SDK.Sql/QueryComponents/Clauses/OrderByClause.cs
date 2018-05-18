using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents.Clauses
{
    public struct OrderByClause
    {
        public string FieldName;
        public Sorting SortOrder;

        public OrderByClause(string field)
        {
            FieldName = field;
            SortOrder = Sorting.Ascending;
        }

        public OrderByClause(string field, Sorting order)
        {
            FieldName = field;
            SortOrder = order;
        }
    }
}