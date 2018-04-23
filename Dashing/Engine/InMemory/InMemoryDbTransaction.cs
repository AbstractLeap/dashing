namespace Dashing.Engine.InMemory {
    using System.Data;
    using System.Data.Common;

    internal class InMemoryDbTransaction : DbTransaction {
        public InMemoryDbTransaction(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            this.DbConnection = connection;
            this.IsolationLevel = isolationLevel;
        }

        public override void Commit() {
            
        }

        public override void Rollback() {
        }

        protected override DbConnection DbConnection { get; }

        public override IsolationLevel IsolationLevel { get; }
    }
}