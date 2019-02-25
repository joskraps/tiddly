namespace Tiddly.Sql.Legacy.QueryComponents.Components
{
    public class SqlLiteral
    {
        public static string StatementRowsAffected = "SELECT @@ROWCOUNT";

        public SqlLiteral(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}