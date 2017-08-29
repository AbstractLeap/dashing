namespace Dashing.CodeGeneration.Weaving
{
    using System.Collections.Generic;

    using Dashing.CodeGeneration.Weaving.Task;
    using Dashing.CodeGeneration.Weaving.Weavers;

    public interface IPeProcessor
    {
        void Process(string peFilePath, IDictionary<string, IEnumerable<ColumnDefinition>> typesToProcess, IEnumerable<IWeaver> weavers);
    }
}