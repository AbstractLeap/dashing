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
        protected ISqlWriter sqlWriter;

        protected bool topHatOwnsTransaction;
        protected bool isDisposed;
        protected bool isCompleted;

        public TopHat(IConfiguration configuration,
            ISqlWriter sqlWriter,
            IDbConnection connection,
            IDbTransaction transaction = null)
        {
            this.connection = connection;
            this.configuration = configuration;
            this.sqlWriter = sqlWriter;

            if (transaction != null)
            {
                topHatOwnsTransaction = false;
                this.transaction = transaction;
            }
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

        public ISqlWriter SqlWriter
        {
            get { return this.sqlWriter; }
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
            this.sqlWriter.Execute(query);
        }

        public void Update<T>(T entity)
        {
            var query = new Query<T> { Entity = entity, QueryType = QueryType.Update };
            this.sqlWriter.Execute(query);
        }

        public void Delete<T>(T entity)
        {
            var query = new Query<T> { Entity = entity, QueryType = QueryType.Delete };
            this.sqlWriter.Execute(query);
        }

        public void Delete<T>(int id)
        {
            throw new NotImplementedException();
        }

        public ISelect<T> Query<T>()
        {
            throw new NotImplementedException();
        }

        public ISelect<T> QueryTracked<T>()
        {
            throw new NotImplementedException();
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
    }
}