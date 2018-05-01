namespace Dashing.Tests.TestDomain.Versioning {
    using System;

    using Dashing.Versioning;

    public class VersionedEntity : IVersionedEntity<string> {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string SessionUser { get; set; }

        public string CreatedBy { get; set; }

        public DateTime SysStartTime { get; set; }

        public DateTime SysEndTime { get; set; }
    }
}