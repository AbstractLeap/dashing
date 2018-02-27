namespace Dashing.Testing.Tests {
    using System;
    using System.Data;

    internal class InMemoryDbConnection : IDbConnection {
        public void Dispose() {
            
        }

        public IDbTransaction BeginTransaction() {
            return new InMemoryDbTransaction(this);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il) {
            return new InMemoryDbTransaction(this);
        }

        public void Close() {
            this.State = ConnectionState.Closed;
        }

        public void ChangeDatabase(string databaseName) {

        }

        public IDbCommand CreateCommand() {
            throw new NotImplementedException();
        }

        public void Open() {
            this.State = ConnectionState.Open;
        }

        public string ConnectionString { get; set; }

        public int ConnectionTimeout { get; }

        public string Database { get; }

        public ConnectionState State { get; private set; }
    }
}