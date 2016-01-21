namespace Dashing.Tests.TestDomain {
    public class ThingWithNullable {
        public int Id { get; set; }

        public int? Nullable { get; set; }

        public string Name { get; set; }
    }

    public class ReferencesThingWithNullable {
        public int Id { get; set; }

        public ThingWithNullable Thing { get; set; }
    }
}