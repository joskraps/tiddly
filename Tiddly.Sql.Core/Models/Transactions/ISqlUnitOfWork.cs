using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Tiddly.Sql.DataAccess
{
    public interface ISqlUnitOfWork
    {
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

        /// <summary>
        ///     Rollbacks this instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void RollbackTransactions();

        /// <summary>
        ///     Commits this instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void CommitTransactions();

        /// <summary>
        ///     Gets the SQL transaction.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        SqlTransaction GetSqlTransaction(string connectionString);

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">connectionType;null</exception>
        IDbConnection GetConnection(string connectionString);

        bool CurrentlyOpen { get; set; }
    }

    /// <summary>
    ///     Handles the logical grouping of transactions from potentially different data sources. The consuming data proxies are
    ///     responsible for enlisting the correct transaction type.
    /// </summary>
    public class SqlUnitOfWork : ISqlUnitOfWork
    {
        private readonly Dictionary<string, IDbTransaction> tranList = new Dictionary<string, IDbTransaction>();

        public UnitOfWorkOptions Options = new UnitOfWorkOptions();

        private bool IsLastTranny => trannyCount == 0;

        private int trannyCount;

        public bool CurrentlyOpen { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see>
        ///         <cref>SqlUnitOfWork</cref>
        ///     </see>
        ///     class.class.
        /// </summary>
        public SqlUnitOfWork()
        {
            Connections = new Dictionary<string, IDbConnection>();
        }

        private Dictionary<string, IDbConnection> Connections { get; }

        /// <summary>
        ///     Begins the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        public ISqlUnitOfWork StartTransaction(Action action)
        {
            return StartTransaction(action, ex => throw ex);

        }

        /// <summary>
        ///     Begins the specified main action.
        /// </summary>
        /// <param name="mainAction">The main action.</param>
        /// <param name="failureAction">The failure action.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ISqlUnitOfWork StartTransaction(Action mainAction, Action<Exception> failureAction)
        {
            try
            {
                CurrentlyOpen = true;
                trannyCount += 1;

                mainAction();

                trannyCount -= 1;

                if (Options.AutoCommit && IsLastTranny)
                    CommitTransactions();
            }
            catch (Exception ex)
            {
                trannyCount -= 1;

                if (Options.AutoRollback)
                    RollbackTransactions();

                failureAction(ex);
            }

            return this;
        }

        /// <summary>
        ///     Rollbacks this instance.
        /// </summary>
        public void RollbackTransactions()
        {
            foreach (var tran in tranList)
            {
                tran.Value.Rollback();
            }
            foreach (var connection in Connections.Keys)
            {
                Connections[connection].Close();
            }

            tranList.Clear();
            Connections.Clear();

            CurrentlyOpen = false;
        }

        /// <summary>
        ///     Commits this instance.
        /// </summary>
        public void CommitTransactions()
        {
            foreach (var tran in tranList)
            {
                tran.Value.Commit();
            }

            foreach (var connection in Connections.Keys)
            {
                Connections[connection].Close();
            }

            tranList.Clear();
            Connections.Clear();

            CurrentlyOpen = false;
        }

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public IDbConnection GetConnection(string connectionString)
        {
            if (!Connections.ContainsKey(connectionString))
            {
                Connections.Add(connectionString, null);
            }


            if (Connections[connectionString] != null)
                return Connections[connectionString];

            var returnConnection = new SqlConnection(connectionString);

            Connections[connectionString] = returnConnection;

            returnConnection.Open();

            tranList.Add(returnConnection.ConnectionString, returnConnection.BeginTransaction());

            return returnConnection;
        }

        /// <summary>
        ///     Gets the SQL transaction.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public SqlTransaction GetSqlTransaction(string connectionString)
        {
            return tranList[connectionString] as SqlTransaction;
        }
    }


    public class UnitOfWorkOptions
    {
        public bool AutoCommit = true;

        public bool AutoRollback = true;
    }
}
