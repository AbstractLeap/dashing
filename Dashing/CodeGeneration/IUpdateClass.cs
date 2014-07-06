namespace Dashing.CodeGeneration {
    using System.Collections.Generic;

    public interface IUpdateClass {
        IList<string> UpdatedProperties { get; set; }
    }
}