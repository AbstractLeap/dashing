namespace TopHat.Engine {
    public class MySqlDialect : SqlDialectBase {
        public MySqlDialect()
            : base('`', '`') { }
    }
}