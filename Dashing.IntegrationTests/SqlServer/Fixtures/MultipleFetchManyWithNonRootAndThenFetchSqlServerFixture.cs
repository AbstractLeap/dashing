namespace Dashing.IntegrationTests.SqlServer.Fixtures {
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.IntegrationTests.TestDomain.More.MultipleFetchManyWithNonRootAndThenFetchDomain;
    using Dashing.Tools;
    using Dashing.Tools.Migration;

    using Moq;

    public class MultipleFetchManyWithNonRootAndThenFetchSqlServerFixture : IDisposable {
        public ISession Session { get; set; }

        public string DatabaseName { get; private set; }

        private readonly IConfiguration config;

        public MultipleFetchManyWithNonRootAndThenFetchSqlServerFixture() {
            this.config = new MultipleFetchManyWithNonRootAndThenFetchConfig();
            this.DatabaseName = "DashingIntegration_" + Guid.NewGuid().ToString("D").Substring(0, 8);

            // load the data
            using (var transactionLessSession = this.config.BeginTransactionLessSession()) {
                var dialect = new SqlServer2012Dialect();
                var migrator = new Migrator(
                    dialect,
                    new CreateTableWriter(dialect),
                    new AlterTableWriter(dialect),
                    new DropTableWriter(dialect),
                    new StatisticsProvider(null, dialect));
                IEnumerable<string> warnings, errors;
                var createStatement = migrator.GenerateSqlDiff(
                    new List<IMap>(),
                    this.config.Maps,
                    null,
                    new Mock<ILogger>().Object,
                    new string[0],
                    new string[0], 
                    out warnings,
                    out errors);
                transactionLessSession.Dapper.Execute("create database " + this.DatabaseName);
                transactionLessSession.Dapper.Execute("use " + this.DatabaseName);
                transactionLessSession.Dapper.Execute(createStatement);
            }

            this.Session = this.config.BeginSession();
            this.Session.Dapper.Execute("use " + this.DatabaseName);
            this.InsertData();
        }

        private void InsertData() {
            this.Session.Insert(new Questionnaire { Name = "Foo" });
            this.Session.Insert(new Question { Questionnaire = new Questionnaire { QuestionnaireId = 1 }, Name = "Bar" });
            this.Session.Insert(new Booking());
            this.Session.Insert(new Room { Name = "Room 1" });
            this.Session.Insert(new RoomSlot { Room = new Room { RoomId = 1 } });
            this.Session.Insert(new Bed { RoomSlot = new RoomSlot { RoomSlotId = 1 }, Booking = new Booking { BookingId = 1 } });
            this.Session.Insert(
                new QuestionnaireResponse { Booking = new Booking { BookingId = 1 }, Questionnaire = new Questionnaire { QuestionnaireId = 1 } });
            this.Session.Insert(
                new QuestionResponse {
                                         Question = new Question { QuestionId = 1 },
                                         QuestionnaireResponse = new QuestionnaireResponse { QuestionnaireResponseId = 1 }
                                     });
        }

        public void Dispose() {
            this.Session.Dispose();
            using (var transactionLessSession = this.config.BeginTransactionLessSession()) {
                transactionLessSession.Dapper.Execute("drop database " + this.DatabaseName);
            }
        }
    }

    public class MultipleFetchManyWithNonRootAndThenFetchConfig : DefaultConfiguration {
        public MultipleFetchManyWithNonRootAndThenFetchConfig()
            : base(new ConnectionStringSettings("Default", "Data Source=(localdb)\\v11.0;Integrated Security=true", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Questionnaire>();
        }
    }
}