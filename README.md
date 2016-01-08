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

We've designed Dashing with productivity and performance in mind. It attempts to make the regular
Sql stuff as simple as possible. So, that's [getting data out](https://github.com/Polylytics/dashing/wiki/Selecting-Data), performing [CRUD](https://github.com/Polylytics/dashing/wiki/Saving-And-Deleting) operations as well as
making code changes as simple as possible - that means strongly typed (for compile time errors and being re-factor friendly) as much
as possible as well as a [simple tool](https://github.com/Polylytics/dashing/wiki/Dbm) for performing automatic migrations of the schema.

### Some Implementation Details

Nearly every ORM available makes use of some proxying technology in order to inject behaviour in to
the domain entities. We make use of this type of functionality in several places. Unlike some we, however,
use IL re-writing at compile time to inject behaviour in to your entities as opposed to proxying them
at runtime. This lets us do some useful things:

#### Implement Equals and GetHashCode

This is basically a developer convenience as it means you don't have to implement this yourself. The 
implementations are aware of auto-generated ID properties and, therefore, ensure that 2 entities
with the same primary key return true for Equals as well as the same hashcode. On top of that, if
the entity has not yet had its primary key generated the hashcode is generated using the default
implementation and is not updated when the primary key is i.e. you wouldn't want a hashcode changing 
for an instance in an IDictionary

#### Strongly type Update queries

See [Bulk Queries](https://github.com/Polylytics/dashing/wiki/Bulk-Queries) for details but this lets you write a strongly typed update query:

	session.UpdateAsync<Post>(p => p.Viewed = true, p => p.PostId == 1);
	
#### Change Tracking

We inject each property with change tracking behaviour so that each entity is aware of which properties
have changed. This means, that when we call SaveAsync on an entity we can quickly determine which
properties need adding to the Update query. Indeed, if nothing has changed, then a query is not executed 
at all. 

#### Collection instantiation

As a convenience to developers we instantiate all ICollection properties, inside the constructor, within 
an entity so that you don't have to!
