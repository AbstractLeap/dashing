using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Tests.Engine.DML {
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class WhereClauseWriterPerformanceTests {
        //[Fact(Skip = "Just for performance testing")]
        //public void NewIsQuicker() {
        //    var dialect = new SqlServer2012Dialect();
        //    var config = MakeConfig();
        //    var i = 0;
        //    Expression<Func<Post, bool>> expr = p => p.PostId == i;
        //    var sw2 = new Stopwatch();
        //    for (; i < 1000; i++) {
        //        sw2.Start();
        //        var newWhereClauseWriter = new WhereClauseWriterNew(dialect, config);
        //        var result = newWhereClauseWriter.GenerateSql(new[] { expr }, null);
        //        sw2.Stop();
        //    }

        //    i = 0;
        //    var sw = new Stopwatch();
        //    for (; i < 1000; i++) {
        //        sw.Start();
        //        var whereClauseWriter = new WhereClauseWriter(dialect, config);
        //        var result = whereClauseWriter.GenerateSql(new[] { expr }, null);
        //        sw.Stop();
        //    }

        //    Console.WriteLine(sw.ElapsedTicks);
        //    Console.WriteLine(sw2.ElapsedTicks);
        //    Assert.True(sw2.ElapsedTicks < sw.ElapsedTicks);
        //}

        private static IConfiguration MakeConfig() {
            return new CustomConfig();
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}
