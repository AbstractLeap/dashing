namespace Dashing.Engine.DDL {
    using Dashing.Configuration;

    public interface IAlterTableWriter {
        string AddColumn(IColumn column);

        string DropColumn(IColumn column);

        string ChangeColumnName(IColumn fromColumn, IColumn toColumn);

        string ModifyColumn(IColumn fromColumn, IColumn toColumn);
    }
}