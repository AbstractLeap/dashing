namespace Dashing.Engine.DDL {
    using Dashing.Configuration;

    public interface ICreateTableWriter {
        string CreateTable(IMap map);
    }
}