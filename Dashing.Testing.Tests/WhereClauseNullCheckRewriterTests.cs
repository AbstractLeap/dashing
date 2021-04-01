namespace Dashing.Testing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;

    using Dashing.Testing.Tests.TestDomain;

    using Xunit;

    public class WhereClauseNullCheckRewriterTests {
        [Fact]
        public void DoesNotRewriteSimple() {
            Expression<Func<Course, bool>> exp = c => c.CourseId == 1;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void SimpleEntityEquals() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 };
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && c.Type == new CourseType { CourseTypeId = 1 };
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void AndAlsoAlready() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 } && c.Price > 1000;
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && c.Type == new CourseType { CourseTypeId = 1 } && c.Price > 1000;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void OrElseAlready() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 } || c.Price > 1000;
            Expression<Func<Course, bool>> expectedResult = c => (c.Type != null && c.Type == new CourseType { CourseTypeId = 1 }) || c.Price > 1000;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void Contains() {
            Expression<Func<Course, bool>> exp = c => new[] { 1 }.Contains(c.Type.CourseTypeId);
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && new[] { 1 }.Contains(c.Type.CourseTypeId);
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void Bool() {
            Expression<Func<Booking, bool>> exp = b => b.Student.IsOraAmbassador;
            Expression<Func<Booking, bool>> expectedResult = b => b.Student != null && b.Student.IsOraAmbassador;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void DoesNotRewriteNullCheck() {
            Expression<Func<Booking, bool>> exp = b => b.Student == null;
            Expression<Func<Booking, bool>> expectedResult = b => b.Student == null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void DoesNotRewriteLastNullCheck() {
            Expression<Func<Booking, bool>> exp = b => b.Student.Course == null;
            Expression<Func<Booking, bool>> expectedResult = b => b.Student != null && b.Student.Course == null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void DoesNotRewriteNullCheck2() {
            Expression<Func<Booking, bool>> exp = b => null == b.Student;
            Expression<Func<Booking, bool>> expectedResult = b => null == b.Student;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void DoesNotRewriteNullCheck3() {
            Expression<Func<Student, bool>> exp = s => s.Name == null;
            Expression<Func<Student, bool>> expectedResult = s => s.Name == null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void OrElseScopesNullCheckCorrectly() {
            var author = new User();
            var blogIds = new List<int>();
            Expression<Func<Post, bool>> exp = p => p.Author == author || blogIds.Contains(p.Blog.BlogId);
            Expression<Func<Post, bool>> expectedResult = p => (p.Author != null && p.Author == author) || (p.Blog != null && blogIds.Contains(p.Blog.BlogId));
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void HasValueOnBaseOk() {
            Expression<Func<Comment, bool>> exp = c => c.DeletedDate.HasValue;
            Expression<Func<Comment, bool>> expectedResult = c => c.DeletedDate.HasValue;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void HasValueOnParentOk() {
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate.HasValue;
            Expression<Func<Comment, bool>> expectedResult = c => c.Post != null && c.Post.DeletedDate.HasValue;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void HasValueOnParentWithOr() {
            var userIds = new List<int>();
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate.HasValue || userIds.Contains(c.User.UserId);
            Expression<Func<Comment, bool>> expectedResult = c => (c.Post != null && c.Post.DeletedDate.HasValue) || (c.User != null && userIds.Contains(c.User.UserId));
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNullWorks() {
            Expression<Func<Comment, bool>> exp = c => c.DeletedDate == null;
            Expression<Func<Comment, bool>> expectedResult = c => c.DeletedDate == null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNotNullWorks() {
            Expression<Func<Comment, bool>> exp = c => c.DeletedDate != null;
            Expression<Func<Comment, bool>> expectedResult = c => c.DeletedDate != null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNullOnParentWorks() {
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate == null;
            Expression<Func<Comment, bool>> expectedResult = c => c.Post != null && c.Post.DeletedDate == null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNotNullOnParentWorks() {
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate != null;
            Expression<Func<Comment, bool>> expectedResult = c => c.Post != null && c.Post.DeletedDate != null;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNotNullOnParentWithOrWorks() {
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate != null || c.User.IsEnabled;
            Expression<Func<Comment, bool>> expectedResult = c => (c.Post != null && c.Post.DeletedDate != null) || (c.User != null && c.User.IsEnabled);
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void NullableEqualsNullOnParentWithOrWorks() {
            Expression<Func<Comment, bool>> exp = c => c.Post.DeletedDate == null || c.User.IsEnabled;
            Expression<Func<Comment, bool>> expectedResult = c => (c.Post != null && c.Post.DeletedDate == null) || (c.User != null && c.User.IsEnabled);
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        [Fact]
        public void BinaryComparisonParametersBothSides() {
            Expression<Func<Pair, bool>> exp = c => c.Left == c.Right;
            Expression<Func<Pair, bool>> expectedResult = c => c.Left != null && c.Right != null && c.Left == c.Right;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToDebugString(), rewrittenClause.ToDebugString());
        }

        public class CourseType {
            public virtual int CourseTypeId { get; set; }
        }

        public class Course {
            public virtual int CourseId { get; set; }

            public virtual CourseType Type { get; set; }

            public virtual decimal Price { get; set; }
        }

        public class Booking {
            public virtual Student Student { get; set; }
        }

        public class Student {
            public virtual bool IsOraAmbassador { get; set; }

            public string Name { get; set; }

            public Course Course { get; set; }
        }
    }
}