namespace Dashing.Weaving.Sample.Domain.Tracking {
    using System;

    public class References {
        public int Id { get; set; }

        public GuidPk GuidPk { get; set; }

        public IntPk IntPk { get; set; }

        public StringPk StringPk { get; set; }

        public LongPk LongPk { get; set; }

        public int Foo { get; set; }

        public string Bar { get; set; }

        public Guid? G { get; set; }
    }
}