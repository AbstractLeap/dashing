using System;
using System.Data;

using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat
{
    public class TopHat : ITopHat
    {
        private IDbConnection connection;
        private IDbTransaction transaction;
        private IConfiguration configuration;
        private ISqlWriter sqlWriter;

        public TopHat(IDbConnection connection,
            IDbTransaction transaction,
            IConfiguration configuration,
            ISqlWriter sqlWriter)
        {
            this.connection = connection;
            this.configuration = configuration;
            this.transaction = transaction;
            this.sqlWriter = sqlWriter;
        }

        public IDbConnection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public IDbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
        }

        public IConfiguration Configuration
        {
            get { throw new NotImplementedException(); }
        }

        public ISqlWriter SqlWriter
        {
            get { throw new NotImplementedException(); }
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public void Update<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(T entity)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}