using System;
using TopHat.Configuration;
namespace TopHat.Engine.DDL
{
    public interface IDropTableWriter
    {
        string DropTable(IMap map);
        string DropTableIfExists(IMap map);
    }
}
