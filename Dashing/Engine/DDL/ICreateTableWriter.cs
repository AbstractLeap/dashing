namespace Dashing.Engine.DDL {
    using System.Collections.Generic;

    using Dashing.Configuration;

    public interface ICreateTableWriter {
        string CreateTable(IMap map);

        IEnumerable<string> CreateForeignKeys(IMap map);

        IEnumerable<string> CreateForeignKeys(IEnumerable<ForeignKey> foreignKeys);

        IEnumerable<string> CreateIndexes(IMap map);

        IEnumerable<string> CreateIndexes(IEnumerable<Index> indexes);
    }
}