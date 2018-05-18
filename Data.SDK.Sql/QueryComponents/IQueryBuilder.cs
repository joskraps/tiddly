using System.Data.Common;

namespace Tiddly.Sql.QueryComponents
{
    public interface IQueryBuilder
    {
        DbCommand BuildCommand();

        DbCommand BuildCommand(DbProviderFactory factory);

        string BuildQuery();
        void SetDbProviderFactory(DbProviderFactory factory);
    }
}