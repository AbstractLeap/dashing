namespace Dashing.Tools.Tests.Migration {
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.Migration.Rename1;
    using Dashing.Tools.Tests.Migration.Rename2;

    using Moq;

    using Xunit;

    public class RenameEntityTests {
        [Fact]
        public void RenameWithIdPrimaryKeyWorks() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Entry", DisplayString = "Entry" });


            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"EXEC sp_RENAME [Posts], [Entries];
alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([Id]);
", script);
        }

        [Fact]
        public void RenameWithDifferentPrimaryKeyWorks() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Entry", DisplayString = "Entry" });

            // change the pk name
            to.GetMap<Entry>().PrimaryKey.Name = "EntryId";
            to.GetMap<Entry>().PrimaryKey.DbName = "EntryId";

            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"EXEC sp_RENAME [Posts], [Entries];
EXEC sp_RENAME 'Entries.Id', 'EntryId', 'COLUMN';
alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([EntryId]);
", script);
        }

        [Fact]
        public void RenameWithDifferentPrimaryKeyTypeAndAttemptChangeWorks() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<RenamePkTypeChange.Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Entry", DisplayString = "Entry" });
            answerProvider.Setup(a => a.GetBooleanAnswer(It.IsAny<string>())).Returns(true);

            // change the pk name
            to.GetMap<RenamePkTypeChange.Entry>().PrimaryKey.Name = "EntryId";
            to.GetMap<RenamePkTypeChange.Entry>().PrimaryKey.DbName = "EntryId";

            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"EXEC sp_RENAME [Posts], [Entries];
EXEC sp_RENAME 'Entries.Id', 'EntryId', 'COLUMN';
alter table [Entries] alter column [EntryId] uniqueidentifier not null NEWSEQUENTIALID() primary key;
alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
alter table [PostComments] alter column [PostId] uniqueidentifier null;
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([EntryId]);
", script);
            Assert.NotEmpty(warnings);
            Assert.Equal("Changing DB Type is not guaranteed to work: Post on PostComment", warnings.First());
        }

        [Fact]
        public void RenameWithDifferentPrimaryKeyTypeAndDropRecreateWorks() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<RenamePkTypeChange.Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Entry", DisplayString = "Entry" });
            answerProvider.Setup(a => a.GetBooleanAnswer(It.IsAny<string>())).Returns(false);

            // change the pk name
            to.GetMap<RenamePkTypeChange.Entry>().PrimaryKey.Name = "EntryId";
            to.GetMap<RenamePkTypeChange.Entry>().PrimaryKey.DbName = "EntryId";

            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(Regex.Replace(@"EXEC sp_RENAME [Posts], [Entries];
declare @OBDCommande51728c6d92c4851aa3b01e8a5a12adb nvarchar(1000);
select @OBDCommande51728c6d92c4851aa3b01e8a5a12adb = 'ALTER TABLE [Posts] drop constraint ' + d.name from sys.tables t   
                          join    sys.default_constraints d       
                           on d.parent_object_id = t.object_id  
                          join    sys.columns c      
                           on c.object_id = t.object_id      
                            and c.column_id = d.parent_column_id
                         where t.name = 'Posts' and c.name = 'Id';
execute(@OBDCommande51728c6d92c4851aa3b01e8a5a12adb);
alter table [Posts] drop column [Id];
alter table [Entries] add [EntryId] uniqueidentifier not null NEWSEQUENTIALID() primary key;
alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
declare @OBDCommand934ba0aed0634856ab81048df9205176 nvarchar(1000);
select @OBDCommand934ba0aed0634856ab81048df9205176 = 'ALTER TABLE [PostComments] drop constraint ' + d.name from sys.tables t   
                          join    sys.default_constraints d       
                           on d.parent_object_id = t.object_id  
                          join    sys.columns c      
                           on c.object_id = t.object_id      
                            and c.column_id = d.parent_column_id
                         where t.name = 'PostComments' and c.name = 'PostId';
execute(@OBDCommand934ba0aed0634856ab81048df9205176);
alter table [PostComments] drop column [PostId];
alter table [PostComments] add [PostId] uniqueidentifier null;
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([EntryId]);
", @"@\w+\b", "@Foo"), Regex.Replace(script, @"@\w+\b", "@Foo"));
        }

        [Fact]
        public void DontRenameWorksAndNoWarningWithNoCurrentData() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "__NOTRENAMED", DisplayString = "Foo" });


            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
drop table [Posts];
create table [Entries] ([Id] int not null identity(1,1) primary key, [Content] nvarchar(255) null);
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([Id]);
", script);
        }

        [Fact]
        public void DontRenameWorksAndWarningWithCurrentData() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<Entry>();
            var migrator = MakeMigrator(from, true);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.IsAny<string>(), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "__NOTRENAMED", DisplayString = "Foo" });


            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
drop table [Posts];
create table [Entries] ([Id] int not null identity(1,1) primary key, [Content] nvarchar(255) null);
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([Id]);
", script);
            Assert.NotEmpty(warnings);
            Assert.Equal(
                @"Property Post on PostComment has changed type but the column was not dropped. There is data in that table, please empty that column if necessary",
                warnings.First());
        }

        [Fact]
        public void RenameWithPropertyRename() {
            var from = new MutableConfiguration(ConnectionString).AddNamespaceOf<Post>();
            var to = new MutableConfiguration(ConnectionString).AddNamespaceOf<RenameTypeChangeAndColumn.Entry>();
            var migrator = MakeMigrator(from);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            var answerProvider = new Mock<IAnswerProvider>();
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.Is<string>(s => s.StartsWith("The entity")), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Entry", DisplayString = "Entry" });
            answerProvider.Setup(a => a.GetMultipleChoiceAnswer<string>(It.Is<string>(s => s.StartsWith("The property")), It.IsAny<IEnumerable<MultipleChoice<string>>>()))
                          .Returns(new MultipleChoice<string> { Choice = "Comments", DisplayString = "Comments" });

            var script = migrator.GenerateSqlDiff(@from.Maps, to.Maps, answerProvider.Object, new Mock<ITraceWriter>().Object, new string[0], out warnings, out errors);
            Assert.Equal(@"EXEC sp_RENAME [Posts], [Entries];
alter table [PostComments] drop constraint [fk_PostComment_Post_Post];
EXEC sp_RENAME 'PostComments.Content', 'Comments', 'COLUMN';
alter table [PostComments] add constraint fk_PostComment_Entry_Post foreign key ([PostId]) references [Entries]([Id]);
", script);
        }

        private static Migrator MakeMigrator(IConfiguration config, bool hasRows = false) {
            var mockStatisticsProvider = new Mock<IStatisticsProvider>();
            mockStatisticsProvider.Setup(s => s.GetStatistics(It.IsAny<IEnumerable<IMap>>()))
                                  .Returns(config.Maps.ToDictionary(m => m.Type.Name, m => new Statistics{ HasRows  = hasRows}));
            var migrator = new Migrator(
                new SqlServerDialect(),
                new CreateTableWriter(new SqlServerDialect()),
                new AlterTableWriter(new SqlServerDialect()),
                new DropTableWriter(new SqlServerDialect()),
                mockStatisticsProvider.Object);
            return migrator;
        }

        private static ConnectionStringSettings ConnectionString {
            get {
                return new ConnectionStringSettings("DefaultDb", string.Empty, "System.Data.SqlClient");
            }
        }
    }
}

namespace Dashing.Tools.Tests.Migration.Rename1 {
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

namespace Dashing.Tools.Tests.Migration.Rename2 {
    public class Entry {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Entry Post { get; set; }
    }
}

namespace Dashing.Tools.Tests.Migration.RenamePkTypeChange {
    using System;

    public class Entry {
        public virtual Guid Id { get; set; }

        public virtual string Content { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }

        public virtual Entry Post { get; set; }
    }
}

namespace Dashing.Tools.Tests.Migration.RenameTypeChangeAndColumn {
    using System;

    public class Entry {
        public virtual int Id { get; set; }

        public virtual string Content { get; set; }
    }

    public class PostComment {
        public virtual int Id { get; set; }

        public virtual string Comments { get; set; }

        public virtual Entry Post { get; set; }
    }
}