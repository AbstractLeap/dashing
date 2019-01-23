namespace Dashing.IntegrationTests.Tests {
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Dashing.Tests.Engine.InMemory;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    using Xunit;
    using Xunit.Abstractions;

    public class CUDPerformanceTests {
        private readonly ITestOutputHelper output;

        public CUDPerformanceTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        // if in future you completely change the implementation again go ahead and delete this
        public void CurrentDelegateInvocationIsQuickerThanReflection() {
            // assemble
            var session = this.GetSession() as Session;
            const int iterations = 1000;
            var users = (new byte[10]).Select(b =>
                new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" }
            );
            session.Insert(users);
            var delegateInsert = ((ConcurrentDictionary<Type, Func<Session, IEnumerable, int>>)session.GetType()
                                         .GetField("InsertMethodsOfType", BindingFlags.Static | BindingFlags.NonPublic)
                                         .GetValue(session))[typeof(User)];
            var reflectiveInsert = typeof(Session).GetMethod(nameof(Session.Insert), BindingFlags.NonPublic | BindingFlags.Instance)
                                              .MakeGenericMethod(typeof(User));

            // run once to get the cached method

            // act
            var i = 0;
            var delegateSw = new Stopwatch();
            for (; i < iterations; i++) {
                delegateSw.Start();
                delegateInsert(session, users);
                delegateSw.Stop();
            }

            i = 0;
            var reflectionSw = new Stopwatch();
            for (; i < iterations; i++) {
                reflectionSw.Start();
                reflectiveInsert.Invoke(session, new[] { users });
                reflectionSw.Stop();
            }


            // assert
            output.WriteLine($"Delegate invocation of insert method in {iterations} iterations: {delegateSw.ElapsedTicks} ticks");
            output.WriteLine($"Reflective invocation of insert method in {iterations} iterations: {reflectionSw.ElapsedTicks} ticks");
            Assert.True(delegateSw.ElapsedTicks < reflectionSw.ElapsedTicks);
        }

        private ISession GetSession() {
            var sessionCreator = new InMemoryDatabase(new TestConfiguration());
            var session        = sessionCreator.BeginSession();

            return session;
        }
    }
}