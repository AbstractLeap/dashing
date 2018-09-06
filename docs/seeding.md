It's a common requirement that databases have a set of pre-seeded data in them for general use e.g. a table of countries
or currency codes.

Regardless of what you're seeding you simply need to implement ISeeder. This can then be executed using dash

    public class DashingConfiguration : DefaultConfiguration, ISeeder {
        public DashingConfiguration()
            : base(ConfigurationManager.ConnectionStrings["DefaultDb"]) {
            this.AddNamespaceOf<Blog>();
        }

        public void Seed(ISession session) {
            // insert some data here
        }
    }

In general, you'll want to make sure that this method is idempotent i.e. if you create some data in here it's not created
every time Seed is called. This function is called by the [dbm](Dbm) tool every time
the database is migrated.

You may want to make use of the `InsertOrUpdate<T>` method on the ISession