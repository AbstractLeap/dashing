namespace TopHat.Tests.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;

    using Moq;

    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Engine.DDL;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class EngineBaseTests {
        [Fact]
        public void FirstTest() {
            var sql = string.Empty;
            var wrapper = new Mock<IDapperWrapper>(MockBehavior.Strict);
            wrapper.Setup(m => m.Execute(It.IsAny<string>(), null, null, null)).Returns(1).Callback<string, object, int?, CommandType?>((s, a, b, c) => sql = s);

            var target = this.MakeTarget(new AnsiSqlDialect());
            target.UseMaps(MakeMaps());
            target.CreateTable<User>(wrapper.Object);
            Debug.WriteLine(sql);

            target = this.MakeTarget(new SqlServerDialect());
            target.UseMaps(MakeMaps());
            target.CreateTable<User>(wrapper.Object);
            Debug.WriteLine(sql);

            target = this.MakeTarget(new MySqlDialect());
            target.UseMaps(MakeMaps());
            target.CreateTable<User>(wrapper.Object);
            Debug.WriteLine(sql);
        }

        [Fact]
        public void OtherTest() {
            var sql = string.Empty;
            var wrapper = new Mock<IDapperWrapper>(MockBehavior.Strict);
            wrapper.Setup(m => m.Execute(It.IsAny<string>(), null, null, null)).Returns(1).Callback<string, object, int?, CommandType?>((s, a, b, c) => sql = s);

            var target = this.MakeTarget(new SqlServerDialect());
            target.UseMaps(MakeMaps());
            target.CreateTable<User>(wrapper.Object);
            Debug.WriteLine(sql);
            target.CreateTable<Blog>(wrapper.Object);
            Debug.WriteLine(sql);
            target.CreateTable<Post>(wrapper.Object);
            Debug.WriteLine(sql);
            target.CreateTable<Comment>(wrapper.Object);
            Debug.WriteLine(sql);
        }

        private static IDictionary<Type, IMap> MakeMaps() {
            var mapper = new DefaultMapper(new DefaultConvention());
            IDictionary<Type, IMap> maps = new Dictionary<Type, IMap>();
            maps[typeof(Blog)] = mapper.MapFor<Blog>();
            maps[typeof(Comment)] = mapper.MapFor<Comment>();
            maps[typeof(Post)] = mapper.MapFor<Post>();
            maps[typeof(User)] = mapper.MapFor<User>();
            return maps;
        }

        private TestEngine MakeTarget(ISqlDialect dialect = null) {
            return new TestEngine(dialect ?? new SqlDialectBase());
        }

        private class TestEngine : EngineBase {
            private readonly CreateTableWriter createTableWriter;

            protected ISqlDialect Dialect { get; set; }

            public TestEngine(ISqlDialect dialect) {
                this.Dialect = dialect;
                this.createTableWriter = new CreateTableWriter(dialect);
            }

            protected override IDbConnection NewConnection(string connectionString) {
                throw new NotImplementedException();
            }

            public override IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
                throw new NotImplementedException();
            }

            public override int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
                throw new NotImplementedException();
            }

            public override int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
                throw new NotImplementedException();
            }

            public override int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
                throw new NotImplementedException();
            }

            public void CreateTable<T>(IDapperWrapper wrapper) {
                var sql = this.createTableWriter.CreateTable(this.MapFor<T>());
                wrapper.Execute(sql);
            }

            protected IMap MapFor<T>() {
                return this.MapFor(typeof(T));
            }

            protected IMap MapFor(Type type) {
                IMap map;
                if (!this.Maps.TryGetValue(type, out map)) {
                    throw new Exception("Type not found in maps");
                }

                return map;
            }
        }
    }
}