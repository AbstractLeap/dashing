namespace Dashing.Weaver.Weaving
{
    using System.Collections.Generic;

    using Dashing.Weaver.Weaving.Weavers;

    public interface IPeProcessor
    {
        void Process(string peFilePath, IDictionary<string, IEnumerable<ColumnDefinition>> typesToProcess, IEnumerable<IWeaver> weavers);
    }
}