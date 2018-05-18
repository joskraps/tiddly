using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.QueryComponents.Clauses
{
    public struct TopClause
    {
        public int Quantity;
        public TopUnit Unit;

        public TopClause(int quantity)
        {
            Quantity = quantity;
            Unit = TopUnit.Records;
        }

        public TopClause(int quantity, TopUnit unit)
        {
            Quantity = quantity;
            Unit = unit;
        }
    }
}