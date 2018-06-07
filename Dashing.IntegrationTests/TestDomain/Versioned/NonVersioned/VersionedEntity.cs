namespace Dashing.IntegrationTests.TestDomain.Versioned.NonVersioned {
    using System;

    /// <summary>
    /// Not actually versioned yet but used in the integration tests to add versioning
    /// </summary>
    public class VersionedEntity {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}