namespace Dashing.CodeGeneration {
    using System;

    class DelegateProjectionResult<TProjection> {
        public DelegateProjectionResult(Type[] types, Func<object[], TProjection> mapper) {
            this.Types = types;
            this.Mapper = mapper;
        }

        public Type[] Types { get; }

        public Func<object[], TProjection> Mapper { get; }
    }
}