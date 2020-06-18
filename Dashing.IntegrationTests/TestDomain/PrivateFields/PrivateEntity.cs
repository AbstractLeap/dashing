namespace Dashing.IntegrationTests.TestDomain.PrivateFields {
    using System;

    public class PrivateEntity {
        private Guid id;

        private string name;

        public override string ToString() {
            return $"{id}:{name}";
        }
    }
}