namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;

    using Dashing.Events;

    using Poly.Logging;

    public interface IConfiguration {
        IEnumerable<IMap> Maps { get; }

        IMap<T> GetMap<T>();

        IMap GetMap(Type type);

        bool HasMap<T>();

        bool HasMap(Type type);

        EventHandlers EventHandlers { get; }

        IPolyLogger Logger { get; }
    }
}