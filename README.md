Intro
=================

Dashing is a simple to use mini ORM built on top of [Dapper](https://github.com/StackExchange/dapper-dot-net). 
It aims to provide fantastic productivity while not sacrificing (too much) performance.

We like to think of it as a strongly typed abstraction over Sql so that you get re-factor 
friendly typed functions that (mostly) mimic the behaviour you'd expect from executing Sql through
Dapper itself.


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

Install the package via Nuget:

	Install-Package Dashing

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
		public DashingConfiguration() : base(ConfigurationManager.ConnectionStrings["DashingConnectionString"]) {
			
			this.AddNamespaceOf<Post>();	
		}
	}

Next, you'll want to generate the schema to use with your domain model.  
A folder called "Dashing" should have been added to this solution. 
Go ahead and open the dev-db.ini file and update the connection string to match your database, 
then update the path to the dll/exe that contains your IConfiguration 
and finally specify the full name of the Configuration class.

Once that's done you'll need to build your project and then you can run:
	
	dbm -m -c .\Dashing\dev-db.ini

Ok, that should have created your database as well as the tables. To use the database you just grab a session from the configuration:

	var config = new DashingConfiguration();
	using (var session = config.BeginSession()) {
		var post = await session.GetAsync<Post>(1);
		post.Title = "Whoop";
		await session.SaveAsync(post);
		session.Complete();
	}
	
Tell me more!
=================

Further documentation is available in the [Wiki](https://github.com/Polylytics/dashing/wiki)

[![Join the chat at https://gitter.im/Polylytics/dashing](https://badges.gitter.im/Polylytics/dashing.svg)](https://gitter.im/Polylytics/dashing?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
