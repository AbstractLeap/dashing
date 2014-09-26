namespace Dashing.Tools.Migration {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;

    public class MigratorBase {
        protected static IMap[] GetTableChanges(
            List<IMap> to,
            List<IMap> @from,
            out IMap[] removals,
            out MigrationPair[] matches) {
            var comparer = new TableNameEqualityComparer();
            var additions = to.Except(@from, comparer).ToArray();
            removals = @from.Except(to, comparer).ToArray();
            matches = @from.Join(to, f => f.Table, t => t.Table, MigrationPair.Of).ToArray();
            return additions;
        }

        protected void AppendSemiColonIfNecesssary(StringBuilder sql) {
            if (sql[sql.Length - 1] != ';') {
                sql.Append(";");
            }
        }
    }
}