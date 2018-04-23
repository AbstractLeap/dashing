namespace Dashing.Engine.InMemory {
    using System;
    using System.Data;
    using System.Data.Common;

    internal class InMemoryDbConnection : DbConnection {
        private ConnectionState state;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
            return new InMemoryDbTransaction(this, isolationLevel);
        }

        public override void Close() {
            this.state = ConnectionState.Closed;
        }

        public override void ChangeDatabase(string databaseName) {
            
        }

        public override void Open() {
            this.state = ConnectionState.Open;
        }

        public override string ConnectionString { get; set; }

        public override string Database { get; }

        public override ConnectionState State => this.state;

        public override string DataSource { get; }

        public override string ServerVersion { get; }

        protected override DbCommand CreateDbCommand() {
            throw new NotImplementedException();
        }
    }
}