namespace Dashing.IntegrationTests.Tests {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class ForUpdateTests {
        //[Fact(Skip = "Has Explicit Delay")]
        //public void LockPreventsRead(TestSessionWrapper wrapper) {
        //    wrapper.Session.Complete();
        //    wrapper.Session.Dispose();
        //    var session1 = wrapper.Session.Configuration.BeginSession();
        //    session1.Dapper.Execute("use " + wrapper.DatabaseName);
        //    var session2 = wrapper.Session.Configuration.BeginSession();
        //    session2.Dapper.Execute("use " + wrapper.DatabaseName);

        //    var start = DateTime.UtcNow;
        //    var task1 = Task.Run(
        //        () => {
        //            var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
        //            session1.Dapper.Execute("waitfor delay '00:00:05';");
        //            session1.Dispose();
        //        });
        //    Thread.Sleep(1000);
        //    var task2 = Task.Run(
        //        () => {
        //            var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
        //            session2.Dispose();
        //            return DateTime.UtcNow;
        //        });
        //    Task.WhenAll(task1, task2).Wait();

        //    Assert.True((task2.Result - start).TotalMilliseconds > 4900);

        //    // do some clear up
        //    //wrapper.Session = wrapper.Session.Configuration.BeginSession();
        //    wrapper.Session.Dapper.Execute("use " + wrapper.DatabaseName);
        //}

        //[Fact(Skip = "Has Explicit Delay")]
        //public void LockEnsuresDataReturnedIsUpToDate(TestSessionWrapper wrapper) {
        //    wrapper.Session.Complete();
        //    wrapper.Session.Dispose();
        //    var session1 = wrapper.Session.Configuration.BeginSession();
        //    session1.Dapper.Execute("use " + wrapper.DatabaseName);
        //    var session2 = wrapper.Session.Configuration.BeginSession();
        //    session2.Dapper.Execute("use " + wrapper.DatabaseName);

        //    var task1 = Task.Run(
        //        () => {
        //            var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
        //            post.Title = "I was Locked!";
        //            Thread.Sleep(2000);
        //            session1.Save(post);
        //            session1.Dapper.Execute("waitfor delay '00:00:05';");
        //            session1.Complete();
        //            session1.Dispose();
        //        });
        //    Thread.Sleep(1000);
        //    var task2 = Task.Run(
        //        () => {
        //            var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
        //            session2.Dispose();
        //            return thisPost.Title;
        //        });
        //    Task.WhenAll(task1, task2).Wait();

        //    Assert.Equal("I was Locked!", task2.Result);

        //    // do some clear up
        //    //wrapper.Session = wrapper.Session.Configuration.BeginSession();
        //    wrapper.Session.Dapper.Execute("use " + wrapper.DatabaseName);
        //}

        //[Fact(Skip = "Has Explicit Delay")]
        //public void LockNotForUpdateReturnsDirty(TestSessionWrapper wrapper) {
        //    wrapper.Session.Complete();
        //    wrapper.Session.Dispose();
        //    var session1 = wrapper.Session.Configuration.BeginSession();
        //    session1.Dapper.Execute("use " + wrapper.DatabaseName);
        //    var session2 = wrapper.Session.Configuration.BeginSession();
        //    session2.Dapper.Execute("use " + wrapper.DatabaseName);

        //    var task1 = Task.Run(
        //        () => {
        //            var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
        //            Thread.Sleep(2000);
        //            post.Title = "I was Locked!";
        //            session1.Save(post);
        //            session1.Dapper.Execute("waitfor delay '00:00:05';");
        //            session1.Complete();
        //            session1.Dispose();
        //        });
        //    Thread.Sleep(1000);
        //    var task2 = Task.Run(
        //        () => {
        //            var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).SingleOrDefault();
        //            session2.Dispose();
        //            return thisPost.Title;
        //        });
        //    Task.WhenAll(task1, task2).Wait();

        //    Assert.NotEqual("I was Locked!", task2.Result);

        //    // do some clear up
        //    //wrapper.Session = wrapper.Session.Configuration.BeginSession();
        //    wrapper.Session.Dapper.Execute("use " + wrapper.DatabaseName);
        //}
    }
}