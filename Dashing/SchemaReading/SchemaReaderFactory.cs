namespace Dashing.SchemaReading {
    using System;

    public class SchemaReaderFactory {
        public ISchemaReader GetSchemaReader(string providerName) {
            if (string.IsNullOrWhiteSpace(providerName)) {
                throw new ArgumentNullException("providerName");
            }

            switch (providerName) {
                case "System.Data.SqlClient":
                    return new SqlServerSchemaReader();

                case "MySql.Data.MySqlClient":
                    return new MySqlServerSchemaReader();
            }

            throw new NotSupportedException(string.Format("dbm does not support the {0} provider", providerName));
        }
    }
}