namespace Dashing {
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DoNotWeaveAttribute : Attribute {}
}