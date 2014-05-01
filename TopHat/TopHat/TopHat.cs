using Dapper;
using System;
using System.Data;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat
{
    public class TopHat : ITopHat
    {
        protected IDbConnection connection;
        protected IDbTransaction transaction;
        protected IConfiguration configuration;

        protected bool topHatOwnsTransaction;
        protected bool isDisposed;
        protected bool isCompleted;

        public TopHat(IConfiguration configuration,
            IDbConnection connection,
            IDbTransaction transaction = null)
        {
            this.connection = connection;
            this.configuration = configuration;

            if (transaction != null)
            {
                this.topHatOwnsTransaction = false;
                this.transaction = transaction;
            }
            else
            {
                this.topHatOwnsTransaction = true;
            }
        }

        public TopHat(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.connection = this.configuration.GetSqlConnection();
            this.topHatOwnsTransaction = true;
        }

        public IDbConnection Connection
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TopHat");
                }

                if (this.connection == null)
                {
                    throw new NullReferenceException("The DBConnection is null");
                }

                if (this.connection.State != ConnectionState.Open)
                {
                    if (this.connection.State == ConnectionState.Closed)
                    {
                        this.connection.Open();
                    }
                    else
                    {
                        throw new Exception("Connection in unknown state");
                    }
                }

                return this.connection;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TopHat");
                }

                if (this.transaction == null)
                {
                    this.transaction = this.Connection.BeginTransaction();
                }

                return this.transaction;
            }
        }

        public IConfiguration Configuration
        {
            get { return this.configuration; }
        }

        public void Complete()
        {
            if (this.isCompleted)
            {
                throw new InvalidOperationException("Only call complete once, when all of the transactional work is done");
            }

            // let's commit the transaction now
            if (this.topHatOwnsTransaction && this.transaction != null)
            {
                this.Transaction.Commit();
            }

            this.isCompleted = true;
        }

        public void Insert<T>(T entity)
        {
            var query = new Query<T> { Entity = entity, QueryType = QueryType.Insert };
            this.ExecuteQuery(query);
        }

        public void Update<T>(T entity)
        {
            var query = new Query<T> { Entity = entity, QueryType = QueryType.Update };
            this.ExecuteQuery(query);
        }

        public IWhereExecute<T> Update<T>()
        {
            return new WhereExecuter<T>(this, QueryType.Update);
        }

        public void Delete<T>(T entity)
        {
            var query = new Query<T> { Entity = entity, QueryType = QueryType.Delete };
            this.ExecuteQuery(query);
        }

        public void Delete<T>(int id)
        {
            throw new NotImplementedException();
        }

        public IWhereExecute<T> Delete<T>()
        {
            return new WhereExecuter<T>(this, QueryType.Delete);
        }

        public ISelect<T> Query<T>()
        {
            return new QueryWriter<T>(this, false);
        }

        public ISelect<T> QueryTracked<T>()
        {
            return new QueryWriter<T>(this, true);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            if (this.topHatOwnsTransaction && this.transaction != null)
            {
                if (!this.isCompleted)
                {
                    this.Transaction.Rollback();
                }

                this.transaction.Dispose();
            }

            this.isDisposed = true;
        }

        private void ExecuteQuery<T>(Query<T> query)
        {
            var sqlQuery = this.Configuration.GetSqlWriter().Execute(query);
            this.Connection.Execute(sqlQuery.Sql, sqlQuery.Parameters);
        }
    }
}