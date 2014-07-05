using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Tests.Configuration.DapperMapperGeneration {
    using System.Collections;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Xunit;

    public class Sandbox {

        [Fact]
        public void LambdaTests() {
            Func<int, Func<int, int>> fac = (int a) => (int b) => a + b;
            Debug.WriteLine(fac(100)(123));

            var ps = Expression.Parameter(typeof(int), "s");
            var pt = Expression.Parameter(typeof(int), "t");
            var ex1 = Expression.Lambda(
                    Expression.Lambda(
                        Expression.Add(ps, pt),
                    pt),
                ps);

            var f1a = (Func<int, Func<int, int>>)ex1.Compile();
            var f1b = f1a(100);
            var f1c = f1a(200);
            Debug.WriteLine(f1b(123));
            Debug.WriteLine(f1c(2));

            var ex2 = Expression.Lambda(
            Expression.Quote(
                Expression.Lambda(
                    Expression.Add(ps, pt),
                pt)),
            ps);

            var f2a = (Func<int, Expression<Func<int, int>>>)ex2.Compile();
            var f2b = f2a(200).Compile();
            Debug.WriteLine(f2b(123));
        }

        [Fact]
        public void LambdaReferenceTest() {
            var foo = new Foo();
            IList<Foo> coll = new List<Foo>();

            var p1 = Expression.Parameter(foo.GetType(), "f");
            var p2 = Expression.Parameter(typeof(IList<>).MakeGenericType(typeof(Foo)), "c");
            var p3 = Expression.Parameter(foo.GetType(), "if");

            var ex = Expression.Lambda(Expression.Lambda(Expression.Block(Expression.Call(p2, typeof(ICollection<>).MakeGenericType(typeof(Foo)).GetMethod("Add"), new Expression[] { p3 }), p3), p3), p1, p2);

            var meth = (Func<Foo, IList<Foo>, Func<Foo, Foo>>)ex.Compile();

            var mapper = meth(foo, coll);
            mapper(new Foo { FooId = 2 });
            mapper(new Foo { FooId = 3 });
        }

        [Fact]
        public void DictionaryCasting() {
            IDictionary<string, Foo2> d1 = new Dictionary<string, Foo2>();
            IDictionary<string, Foo> d2 = (IDictionary<string, Foo>)d1;

        }

        class Foo2 : Foo {
            
        }

        class Foo {
            public int FooId { get; set; }
        }
    }
}
