using System.Data.Common;

namespace Tiddly.Sql.Legacy.QueryComponents
{
    public interface IQueryBuilder
    {
        void SetDbProviderFactory(DbProviderFactory factory);

        string BuildQuery();

        DbCommand BuildCommand();

        DbCommand BuildCommand(DbProviderFactory factory);
    }
}