using Tiddly.Sql.QueryComponents;

namespace Tiddly.Sql.Models
{
    public class SqlQueryBuilder
    {
        private DeleteQueryBuilder deleteBuilder;
        private InsertQueryBuilder insertQueryBuilder;
        private SelectQueryBuilder selectQueryBuilder;
        private UpdateQueryBuilder updateBuilder;

        public DeleteQueryBuilder DeleteBuilder =>
            deleteBuilder ?? (deleteBuilder = new DeleteQueryBuilder());

        public InsertQueryBuilder InsertBuilder =>
            insertQueryBuilder ?? (insertQueryBuilder = new InsertQueryBuilder());

        public SelectQueryBuilder SelectQueryBuilder =>
            selectQueryBuilder ?? (selectQueryBuilder = new SelectQueryBuilder());

        public UpdateQueryBuilder UpdateBuilder =>
            updateBuilder ?? (updateBuilder = new UpdateQueryBuilder());
    }
}