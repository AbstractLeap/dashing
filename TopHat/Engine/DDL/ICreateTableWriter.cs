using System;
using TopHat.Configuration;
namespace TopHat.Engine.DDL
{
    public interface ICreateTableWriter
    {
        string CreateTable(IMap map);
    }
}
