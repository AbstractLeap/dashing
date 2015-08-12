Intro
=================

Dashing is a simple to use mini ORM built on top of [Dapper](https://github.com/StackExchange/dapper-dot-net). It aims to provide fantastic productivity while not sacrificing (too much) speed.

We'd like to think it sits somewhere between Dapper and NHibernate. So, it's a little bit more productive than Dapper but focused on speed (and even lets you go back to Dapper should the need arise.)

Features
-----------------

* Simple Configuration
* Sql-like strongly typed query syntax
* Eager loading of relationships
* Change Tracking
* Crud Operations
* Schema Generation/Migrations
* Multiple Database Support (SQL Server/MySql right now)
* Async Support

Getting Started
=================

Dashing is code first so simply create your domain models using POCOs e.g.

	class Blog {
		public int BlogId { get;set; }

		public string Title { get; set; }

		public IList<Post> Posts { get; set; }
	}

	class Post {
		public int PostId { get; set; }

		public string Title	{ get; set; }

		public DateTime Date { get; set; }

		public Blog Blog { get; set; }
	}

and then create your configuration (you'll need to put the connection string in your web.config or app.config):

	class DashingConfiguration: DefaultConfiguration {
		public DashingConfiguration() : base(ConfigurationManager.ConnectionStrings["MyConnectionString"]) {
			
			this.AddNamespaceOf<Post>();	
		})
	}

Next, you'll want to generate the database to use with your domain model. A folder called "Dashing" should have been added to this solution. Go ahead and open the dev-db.ini file and update the connection string to match your database, then update the path to the dll that contains your IConfiguration and finally specify the full name of the Configuration class.

Once that's done you can run:
	
	dbm -m -c ./Dashing/dev-db.ini

Ok, you're all set up. To use the database you just grab a session from the configuration:

	var config = new DashingConfiguration();
	using (var session = config.BeginSession()) {
		var post = session.Get(1);
		post.Title = "Whoop";
		session.Save(post);
		session.Complete();
	}