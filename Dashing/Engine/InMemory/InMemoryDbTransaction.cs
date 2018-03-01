namespace Dashing.Engine.InMemory {
    using System.Data;

    internal class InMemoryDbTransaction : IDbTransaction {
        public InMemoryDbTransaction(IDbConnection connection, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            this.Connection = connection;
            this.IsolationLevel = isolationLevel;
        }

        public void Dispose() { }

        public void Commit() { }

        public void Rollback() { }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }
    }
}