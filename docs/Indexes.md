Adding indexes to your database schema is done using the configuration object. These 
indexes can then be created using the dash tool.

By default indexes are automatically created on all properties that are many to one relations. 
This is done on the premise that you will be using these relationships in fetch queries and that 
these will benefit from the indexes.

To specifically add a new index use the Index() method on a particular IMap instance. 
For example, if you'd like to index the Title property on your Blog class you would set 
up your configuration like so:

	public class DashingConfiguration : DefaultConfiguration {
        public DashingConfiguration() {
			// add the domain classes to the config
            this.AddNamespaceOf<Blog>();

            // add any extra indexes
            this.Setup<Blog>().Index(b => new { b.Title }, false);
        }
    }

Notice that the second argument to the function indicates whether you would like the index to be unique or not. 

Note: By default any indexes that contain nullable columns will generate a
 [filtered index](http://msdn.microsoft.com/en-gb/library/cc280372.aspx) in sql server.