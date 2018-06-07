namespace Dashing.Engine.DDL {
    using Dashing.Configuration;

    public interface IAlterTableWriter {
        string AddColumn(params IColumn[] columns);

        string DropColumn(IColumn column);

        string ChangeColumnName(IColumn fromColumn, IColumn toColumn);

        string ModifyColumn(IColumn fromColumn, IColumn toColumn);

        string DropForeignKey(ForeignKey foreignKey);

        string DropIndex(Index index);

        string RenameTable(IMap @from, IMap to);

        string AddSystemVersioning(IMap to);
    }
}