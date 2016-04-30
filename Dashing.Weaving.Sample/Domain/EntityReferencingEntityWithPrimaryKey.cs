namespace Dashing.Weaving.Sample.Domain {
    public class EntityReferencingEntityWithPrimaryKey {
        public long Id { get; set; }

        public string Name { get; set; }

        public EntityWithStringPrimaryKey EntityWithStringPrimaryKey { get; set; }
    }
}