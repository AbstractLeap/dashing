namespace PerformanceTest {
    using System;

    internal class Test {
        public Test(string provider, string name, Action<int> func, string method = null) {
            this.Provider = provider;
            this.TestName = name;
            this.TestFunc = func;
            this.Method = method;
        }

        /// <remarks>Prevents 'return value of pure method is not used' warnings</remarks>
        public Test(string provider, string name, Func<int, object> func, string method = null)
            : this(provider, name, i => { var forceEvaluation = func(i) != null; }, method) { }

        public string Provider { get; private set; }

        public string Method { get; private set; }

        public string TestName { get; private set; }

        public Action<int> TestFunc { get; private set; }
    }
}