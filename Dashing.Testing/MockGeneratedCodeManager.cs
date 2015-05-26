namespace Dashing.Testing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.Engine.DML;

    public class MockGeneratedCodeManager : IGeneratedCodeManager {
        public Type GetForeignKeyType(Type type) {
            throw new NotImplementedException();
        }

        public Type GetTrackingType(Type type) {
            throw new NotImplementedException();
        }

        public Type GetUpdateType(Type type) {
            throw new NotImplementedException();
        }

        public T CreateForeignKeyInstance<T>() {
            throw new NotImplementedException();
        }

        public T CreateTrackingInstance<T>() {
            return Activator.CreateInstance<T>();
        }

        public T CreateTrackingInstance<T>(T entity) {
            return entity;
        }

        public T CreateUpdateInstance<T>() {
            throw new NotImplementedException();
        }

        public void TrackInstance<T>(T entity) {}

        public IEnumerable<T> Query<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(SqlWriterResult result, IDbConnection connection, IDbTransaction transaction, bool asTracked = false) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null) {
            throw new NotImplementedException();
        }

        public int Execute(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            throw new NotImplementedException();
        }

        public T QueryScalar<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(SqlWriterResult sqlQuery, IDbConnection connection, IDbTransaction transaction, bool asTracked = false) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null) {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteAsync(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            throw new NotImplementedException();
        }

        public Task<T> QueryScalarAsync<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null) {
            throw new NotImplementedException();
        }
    }
}