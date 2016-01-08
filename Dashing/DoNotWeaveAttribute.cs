namespace Dashing {
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DoNotWeaveAttribute : Attribute {
    }
}