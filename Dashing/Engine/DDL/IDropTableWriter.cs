using System;
using Dashing.Configuration;
namespace Dashing.Engine.DDL
{
    public interface IDropTableWriter
    {
        string DropTable(IMap map);
        string DropTableIfExists(IMap map);
    }
}
