namespace Dashing.Engine.DML {
    using Dapper;

    public class AutoNamingDynamicParameters : DynamicParameters {
        private int paramCounter;

        public string Add(object value) {
            var name = "@l_" + ++this.paramCounter;
            this.Add(name, value);
            return name;
        }
    }
}