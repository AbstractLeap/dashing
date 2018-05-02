using Dashing.Tests.TestDomain;

namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Migration;
    using Dashing.Tests;
    using Dashing.Tests.TestDomain.Guid;
    using Dashing.Tests.TestDomain.Versioning;
    using Dashing.Tools.TestDomain;
    using Dashing.Tools.Tests.Migration.Simple;
    using Dashing.Tools.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class MigrationCreateTests {
        private readonly ITestOutputHelper output;

        public MigrationCreateTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void VersionedEntityWorks() {
            var config = new MutableConfiguration();
            config.Add<VersionedEntity>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            this.output.WriteLine(script);
            Assert.Equal(
                "create table [VersionedEntities] ([Id] uniqueidentifier not null DEFAULT NEWSEQUENTIALID() primary key, [Name] nvarchar(255) null, [SessionUser]  as (cast(SESSION_CONTEXT(N'UserId') as nvarchar)), [CreatedBy] nvarchar(255) NULL DEFAULT (cast(SESSION_CONTEXT(N'UserId') as nvarchar)), [SysStartTime] datetime2(2) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL, [SysEndTime] datetime2(2) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL, PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)) WITH (SYSTEM_VERSIONING = ON ( HISTORY_TABLE = dbo.[VersionedEntitiesHistory]));",
                script.Trim());
        }

        [Fact]
        public void GuidPrimaryKeyWorks()
        {
            var config = new MutableConfiguration();
            config.Add<EntityWithGuidPk>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            this.output.WriteLine(script);
            Assert.Equal(
                "create table [EntityWithGuidPks] ([Id] uniqueidentifier not null DEFAULT NEWSEQUENTIALID() primary key, [Name] nvarchar(255) null);",
                script.Trim());
        }

        [Fact]
        public void CreateTableWorks() {
            var config = new SimpleClassConfig();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(
                "create table [SimpleClasses] ([SimpleClassId] int not null identity(1,1) primary key, [Name] nvarchar(255) null, [CreatedDate] datetime2(2) not null default (current_timestamp));",
                script.Trim());
        }

        [Fact]
        public void SelfReferencingWorks() {
            var config = new MutableConfiguration();
            config.Add<Category>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Categories] ([CategoryId] int not null identity(1,1) primary key, [ParentId] int null, [Name] nvarchar(255) null);
alter table [Categories] add constraint fk_Category_Category_Parent foreign key ([ParentId]) references [Categories]([CategoryId]);
create index [idx_Category_Parent] on [Categories] ([ParentId]);",
                @"(?<!\r)\n",
                Environment.NewLine),
                script.Trim());
        }

        [Fact]
        public void PairWorks() {
            var config = new MutableConfiguration();
            config.Add<Pair>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Pairs] ([PairId] int not null identity(1,1) primary key, [ReferencesId] int null, [ReferencedById] int null);
alter table [Pairs] add constraint fk_Pair_Pair_References foreign key ([ReferencesId]) references [Pairs]([PairId]);
alter table [Pairs] add constraint fk_Pair_Pair_ReferencedBy foreign key ([ReferencedById]) references [Pairs]([PairId]);
create index [idx_Pair_References] on [Pairs] ([ReferencesId]);
create index [idx_Pair_ReferencedBy] on [Pairs] ([ReferencedById]);",
                @"(?<!\r)\n",
                Environment.NewLine),
                script.Trim());
        }

        [Fact]
        public void OneToOneWorks() {
            var config = new MutableConfiguration();
            config.Add<OneToOneLeft>();
            config.Add<OneToOneRight>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [OneToOneLefts] ([OneToOneLeftId] int not null identity(1,1) primary key, [RightId] int null, [Name] nvarchar(255) null);
create table [OneToOneRights] ([OneToOneRightId] int not null identity(1,1) primary key, [LeftId] int null, [Name] nvarchar(255) null);
alter table [OneToOneLefts] add constraint fk_OneToOneLeft_OneToOneRight_Right foreign key ([RightId]) references [OneToOneRights]([OneToOneRightId]);
alter table [OneToOneRights] add constraint fk_OneToOneRight_OneToOneLeft_Left foreign key ([LeftId]) references [OneToOneLefts]([OneToOneLeftId]);
create index [idx_OneToOneLeft_Right] on [OneToOneLefts] ([RightId]);
create index [idx_OneToOneRight_Left] on [OneToOneRights] ([LeftId]);",
                @"(?<!\r)\n",
                Environment.NewLine),
                script.Trim());
        }

        [Fact]
        public void ComplexDomainBuilds() {
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Dashing.Tests.TestDomain.Post>();
            var migrator = MakeMigrator(config);
            IEnumerable<string> errors;
            IEnumerable<string> warnings;
            var script = migrator.GenerateSqlDiff(
                new IMap[] { },
                config.Maps,
                null,
                new Mock<ILogger>().Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            this.output.WriteLine(script.Trim());
            Assert.Equal(Regex.Replace(@"create table [Blogs] ([BlogId] int not null identity(1,1) primary key, [Title] nvarchar(255) null, [CreateDate] datetime2(2) not null default (current_timestamp), [Description] nvarchar(255) null, [OwnerId] int null);
create table [BoolClasses] ([BoolClassId] int not null identity(1,1) primary key, [IsFoo] bit not null default (0));
create table [Comments] ([CommentId] int not null identity(1,1) primary key, [Content] nvarchar(255) null, [PostId] int null, [UserId] int null, [CommentDate] datetime2(2) not null default (current_timestamp));
create table [Likes] ([LikeId] int not null identity(1,1) primary key, [UserId] int null, [CommentId] int null);
create table [OneToOneLefts] ([OneToOneLeftId] int not null identity(1,1) primary key, [RightId] int null, [Name] nvarchar(255) null);
create table [OneToOneRights] ([OneToOneRightId] int not null identity(1,1) primary key, [LeftId] int null, [Name] nvarchar(255) null);
create table [Pairs] ([PairId] int not null identity(1,1) primary key, [ReferencesId] int null, [ReferencedById] int null);
create table [Posts] ([PostId] int not null identity(1,1) primary key, [Title] nvarchar(255) null, [Content] nvarchar(255) null, [Rating] decimal(18,10) not null default (0), [AuthorId] int null, [BlogId] int null, [DoNotMap] bit not null default (0));
create table [PostTags] ([PostTagId] int not null identity(1,1) primary key, [PostId] int null, [ElTagId] int null);
create table [Tags] ([TagId] int not null identity(1,1) primary key, [Content] nvarchar(255) null);
create table [ThingWithNullables] ([Id] int not null identity(1,1) primary key, [Nullable] int null, [Name] nvarchar(255) null);
create table [ReferencesThingWithNullables] ([Id] int not null identity(1,1) primary key, [ThingId] int null);
create table [Users] ([UserId] int not null identity(1,1) primary key, [Username] nvarchar(255) null, [EmailAddress] nvarchar(255) null, [Password] nvarchar(255) null, [IsEnabled] bit not null default (0), [HeightInMeters] decimal(18,10) not null default (0));
alter table [Blogs] add constraint fk_Blog_User_Owner foreign key ([OwnerId]) references [Users]([UserId]);
alter table [Comments] add constraint fk_Comment_Post_Post foreign key ([PostId]) references [Posts]([PostId]);
alter table [Comments] add constraint fk_Comment_User_User foreign key ([UserId]) references [Users]([UserId]);
alter table [Likes] add constraint fk_Like_User_User foreign key ([UserId]) references [Users]([UserId]);
alter table [Likes] add constraint fk_Like_Comment_Comment foreign key ([CommentId]) references [Comments]([CommentId]);
alter table [OneToOneLefts] add constraint fk_OneToOneLeft_OneToOneRight_Right foreign key ([RightId]) references [OneToOneRights]([OneToOneRightId]);
alter table [OneToOneRights] add constraint fk_OneToOneRight_OneToOneLeft_Left foreign key ([LeftId]) references [OneToOneLefts]([OneToOneLeftId]);
alter table [Pairs] add constraint fk_Pair_Pair_References foreign key ([ReferencesId]) references [Pairs]([PairId]);
alter table [Pairs] add constraint fk_Pair_Pair_ReferencedBy foreign key ([ReferencedById]) references [Pairs]([PairId]);
alter table [Posts] add constraint fk_Post_User_Author foreign key ([AuthorId]) references [Users]([UserId]);
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([BlogId]);
alter table [PostTags] add constraint fk_PostTag_Post_Post foreign key ([PostId]) references [Posts]([PostId]);
alter table [PostTags] add constraint fk_PostTag_Tag_ElTag foreign key ([ElTagId]) references [Tags]([TagId]);
alter table [ReferencesThingWithNullables] add constraint fk_ReferencesThingWithNullable_ThingWithNullable_Thing foreign key ([ThingId]) references [ThingWithNullables]([Id]);
create index [idx_Blog_Owner] on [Blogs] ([OwnerId]);
create index [idx_Comment_Post] on [Comments] ([PostId]);
create index [idx_Comment_User] on [Comments] ([UserId]);
create index [idx_Like_User] on [Likes] ([UserId]);
create index [idx_Like_Comment] on [Likes] ([CommentId]);
create index [idx_OneToOneLeft_Right] on [OneToOneLefts] ([RightId]);
create index [idx_OneToOneRight_Left] on [OneToOneRights] ([LeftId]);
create index [idx_Pair_References] on [Pairs] ([ReferencesId]);
create index [idx_Pair_ReferencedBy] on [Pairs] ([ReferencedById]);
create index [idx_Post_Author] on [Posts] ([AuthorId]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);
create index [idx_PostTag_Post] on [PostTags] ([PostId]);
create index [idx_PostTag_ElTag] on [PostTags] ([ElTagId]);
create index [idx_ReferencesThingWithNullable_Thing] on [ReferencesThingWithNullables] ([ThingId]);",
                @"(?<!\r)\n",
                Environment.NewLine),
                script.Trim());
        }

        private static Migrator MakeMigrator(IConfiguration config) {
            var mockStatisticsProvider = new Mock<IStatisticsProvider>();
            mockStatisticsProvider.Setup(s => s.GetStatistics(It.IsAny<IEnumerable<IMap>>()))
                                  .Returns(config.Maps.ToDictionary(m => m.Type.Name, m => new Statistics()));
            var migrator = new Migrator(
                new SqlServerDialect(),
                new CreateTableWriter(new SqlServerDialect()),
                new AlterTableWriter(new SqlServerDialect()),
                new DropTableWriter(new SqlServerDialect()),
                mockStatisticsProvider.Object);
            return migrator;
        }

        private class SimpleClassConfig : BaseConfiguration {
            public SimpleClassConfig() {
                this.Add<SimpleClass>();
            }
        }
    }
}