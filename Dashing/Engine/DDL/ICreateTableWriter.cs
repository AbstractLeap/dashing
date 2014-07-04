using System;
using Dashing.Configuration;
namespace Dashing.Engine.DDL
{
    public interface ICreateTableWriter
    {
        string CreateTable(IMap map);
    }
}
