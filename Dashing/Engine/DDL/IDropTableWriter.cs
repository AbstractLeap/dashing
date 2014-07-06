namespace Dashing.Engine.DDL {
    using Dashing.Configuration;

    public interface IDropTableWriter {
        string DropTable(IMap map);

        string DropTableIfExists(IMap map);
    }
}