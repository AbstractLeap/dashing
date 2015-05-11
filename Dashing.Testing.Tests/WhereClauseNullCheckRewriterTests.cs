namespace Dashing.Testing.Tests {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class WhereClauseNullCheckRewriterTests {
        [Fact]
        public void DoesNotRewriteSimple() {
            Expression<Func<Course, bool>> exp = c => c.CourseId == 1;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(exp.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void SimpleEntityEquals() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 };
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && c.Type == new CourseType { CourseTypeId = 1 };
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void AndAlsoAlready() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 } && c.Price > 1000;
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && c.Type == new CourseType { CourseTypeId = 1 } && c.Price > 1000;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void OrElseAlready() {
            Expression<Func<Course, bool>> exp = c => c.Type == new CourseType { CourseTypeId = 1 } || c.Price > 1000;
            Expression<Func<Course, bool>> expectedResult = c => (c.Type != null && c.Type == new CourseType { CourseTypeId = 1 }) || c.Price > 1000;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void Contains() {
            Expression<Func<Course, bool>> exp = c => new[] { 1 }.Contains(c.Type.CourseTypeId);
            Expression<Func<Course, bool>> expectedResult = c => c.Type != null && new[] { 1 }.Contains(c.Type.CourseTypeId);
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
        }

        [Fact]
        public void Bool() {
            Expression<Func<Booking, bool>> exp = b => b.Student.IsOraAmbassador;
            Expression<Func<Booking, bool>> expectedResult = b => b.Student != null && b.Student.IsOraAmbassador;
            var rewriter = new WhereClauseNullCheckRewriter();
            var rewrittenClause = rewriter.Rewrite(exp);
            Assert.Equal(expectedResult.ToString(), rewrittenClause.ToString());
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
        }
    }
}
