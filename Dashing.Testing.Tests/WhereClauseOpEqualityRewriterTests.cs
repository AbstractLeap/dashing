namespace Dashing.Testing.Tests {
    using System;
    using System.Linq.Expressions;

    using Xunit;

    public class WhereClauseOpEqualityRewriterTests {
        [Fact]
        public void DoesRewrite() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 };
            Expression<Func<Course, bool>> expectedResult = c => c.Type.Equals(new CourseType { CourseTypeId = 1 });
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void DoesNotRewriteValueComparisons() {
            Expression<Func<Course, bool>> exp = c => c.Price == 30;
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void DoesNotRewriteStringComparisons() {
            Expression<Func<Course, bool>> exp = c => c.Name == "Foo";
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void DoesNotRewriteNullStringComparisons() {
            Expression<Func<Course, bool>> exp = c => c.Name == null;
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void DoesNotRewriteNullEntityComparisons() {
            Expression<Func<Course, bool>> exp = c => c.Type == null;
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void DoesNotRewriteLastNullEntityComparisons() {
            Expression<Func<Course, bool>> exp = c => c.Type.Category == null;
            var rewriter = new WhereClauseOpEqualityRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        public class CourseType {
            public virtual int CourseTypeId { get; set; }

            public Category Category { get; set; }
        }

        public class Course {
            public virtual int CourseId { get; set; }

            public virtual CourseType Type { get; set; }

            public virtual decimal Price { get; set; }

            public string Name { get; set; }
        }

        public class Category {
            public int Id { get; set; }
        }
    }
}