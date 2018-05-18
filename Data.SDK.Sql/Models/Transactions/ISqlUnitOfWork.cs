using System;
using System.Data;
using System.Data.SqlClient;

namespace Tiddly.Sql.Models.Transactions
{
    public interface ISqlUnitOfWork
    {
        bool CurrentlyOpen { get; set; }

        /// <summary>
        ///     Commits this instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void CommitTransactions();

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">connectionType;null</exception>
        IDbConnection GetConnection(string connectionString);

        /// <summary>
        ///     Gets the SQL transaction.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        SqlTransaction GetSqlTransaction(string connectionString);

        /// <summary>
        ///     Rollbacks this instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void RollbackTransactions();

        /// <summary>
        ///     Begins the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        ISqlUnitOfWork StartTransaction(Action action);

        /// <summary>
        ///     Begins the specified main action.
        /// </summary>
        /// <param name="mainAction">The main action.</param>
        /// <param name="failureAction">The failure action.</param>
        ISqlUnitOfWork StartTransaction(Action mainAction, Action<Exception> failureAction);
    }
}