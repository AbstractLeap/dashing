using System;
using System.Collections.Generic;

namespace TopHat.Configuration
{
    public interface IMapping
    {
        IDictionary<Type, IMap> Maps { get; }
    }
}