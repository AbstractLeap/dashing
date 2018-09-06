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
    using Dashing.Tools.Tests.Migration.Simple;

    using Moq;

    using Xunit;

    public class SimpleTests {
        [Fact]
        public void DropEntityWorks() {
            var from = new MutableConfiguration().AddNamespaceOf<Post>();
            var to = new MutableConfiguration().Add<Post>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                null,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(@"drop table [PostComments];", script.Trim());
        }

        [Fact]
        public void AddEntityWorks() {
            var from = new MutableConfiguration().Add<Post>();
            var to = new MutableConfiguration().AddNamespaceOf<Post>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                null,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [PostComments] ([Id] int not null identity(1,1) primary key, [Content] nvarchar(255) null, [PostId] int null);
alter table [PostComments] add constraint fk_PostComment_Post_Post foreign key ([PostId]) references [Posts]([Id]);
create index [idx_PostComment_Post] on [PostComments] ([PostId]);",
                @"(?<!\r)\n",
                Environment.NewLine),
                script.Trim());
        }

        [Fact]
        public void AddPropertyWorks() {
            var from = new MutableConfiguration().AddNamespaceOf<Post>();
            var to = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                null,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(@"alter table [PostComments] add [Rating] int not null default (0);", script.Trim());
        }

        [Fact]
        public void DropPropertyWorks() {
            var from = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var to = new MutableConfiguration().AddNamespaceOf<Post>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                null,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(
                Regex.Replace(Regex.Replace(@"declare @OBDCommandca45edee90bb4cc68430f8540d28aa99 nvarchar(1000);
select @OBDCommandca45edee90bb4cc68430f8540d28aa99 = 'ALTER TABLE [PostComments] drop constraint ' + d.name from sys.tables t   
                          join    sys.default_constraints d       
                           on d.parent_object_id = t.object_id  
                          join    sys.columns c      
                           on c.object_id = t.object_id      
                            and c.column_id = d.parent_column_id
                         where t.name = 'PostComments' and c.name = 'Rating';
execute(@OBDCommandca45edee90bb4cc68430f8540d28aa99);
alter table [PostComments] drop column [Rating];", @"@\w+\b", "@Foo"), @"(?<!\r)\n", Environment.NewLine),
                Regex.Replace(script.Trim(), @"@\w+\b", "@Foo"));
        }

        [Fact]
        public void AddNewRelationshipToTableWithNoData() {
            var from = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var to = new MutableConfiguration().AddNamespaceOf<Simple3.Post>();
            var migrator = MakeMigrator(from);
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetAnswer<int>(It.IsAny<string>())).Returns(99);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                answerProvider.Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Blogs] ([Id] int not null identity(1,1) primary key, [Title] nvarchar(255) null);
alter table [Posts] add [BlogId] int null;
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([Id]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);", @"(?<!\r)\n", Environment.NewLine), script.Trim());
        }

        [Fact]
        public void AddNewRelationshipToTableWithNoDataAndNotNull() {
            var from = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var to = new MutableConfiguration();
            to.AddNamespaceOf<Simple3.Post>();
            to.Setup<Simple3.Post>().Property(p => p.Blog).IsNullable = false;
            var migrator = MakeMigrator(from);
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetAnswer<int>(It.IsAny<string>())).Returns(99);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                answerProvider.Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Blogs] ([Id] int not null identity(1,1) primary key, [Title] nvarchar(255) null);
alter table [Posts] add [BlogId] int not null;
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([Id]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);", @"(?<!\r)\n", Environment.NewLine), script.Trim());
        }

        [Fact]
        public void AddNewNullRelationshipToTableWithData() {
            var from = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var to = new MutableConfiguration().AddNamespaceOf<Simple3.Post>();
            var migrator = MakeMigrator(from, true);
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetAnswer<int>(It.IsAny<string>())).Returns(99);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                answerProvider.Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Blogs] ([Id] int not null identity(1,1) primary key, [Title] nvarchar(255) null);
alter table [Posts] add [BlogId] int null;
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([Id]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);", @"(?<!\r)\n", Environment.NewLine), script.Trim());
        }

        [Fact]
        public void AddNewNotNullRelationshipToTableWithData() {
            var from = new MutableConfiguration().AddNamespaceOf<Simple2.Post>();
            var to = new MutableConfiguration();
            to.AddNamespaceOf<Simple3.Post>();
            to.Setup<Simple3.Post>().Property(p => p.Blog).IsNullable = false;
            var migrator = MakeMigrator(from, true);
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetAnswer<int>(It.IsAny<string>())).Returns(99);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var script = migrator.GenerateSqlDiff(
                @from.Maps,
                to.Maps,
                answerProvider.Object,
                new string[0],
                new string[0],
                out warnings,
                out errors);
            Assert.Equal(Regex.Replace(@"create table [Blogs] ([Id] int not null identity(1,1) primary key, [Title] nvarchar(255) null);
alter table [Posts] add [BlogId] int not null default (99);
alter table [Posts] add constraint fk_Post_Blog_Blog foreign key ([BlogId]) references [Blogs]([Id]);
create index [idx_Post_Blog] on [Posts] ([BlogId]);", @"(?<!\r)\n", Environment.NewLine), script.Trim());
        }

        private static Migrator MakeMigrator(IConfiguration config, bool hasRows = false) {
            var mockStatisticsProvider = new Mock<IStatisticsProvider>();
            mockStatisticsProvider.Setup(s => s.GetStatistics(It.IsAny<IEnumerable<IMap>>()))
                                  .Returns(config.Maps.ToDictionary(m => m.Type.Name.ToLowerInvariant(), m => new Statistics { HasRows = hasRows }));
            var migrator = new Migrator(
                new SqlServerDialect(),
                new CreateTableWriter(new SqlServerDialect()),
                new AlterTableWriter(new SqlServerDialect()),
                new DropTableWriter(new SqlServerDialect()),
                mockStatisticsProvider.Object);
            return migrator;
        }
    }
}

namespace Dashing.Tools.Tests.Migration.Simple {
    public class Post {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }
    }
}

namespace Dashing.Tools.Tests.Migration.Simple2 {
    public class Post {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }

        public virtual int Rating { get; set; }
    }
}

namespace Dashing.Tools.Tests.Migration.Simple3 {
    public class Blog {
        public virtual int Id { get; set; }

        public virtual string Title { get; set; }
    }

    public class Post {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Blog Blog { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }

        public virtual int Rating { get; set; }
    }
}