Dashing is a simple to use mini ORM built on top of [Dapper](https://github.com/StackExchange/dapper-dot-net). 
It aims to be a strongly typed data access layer that is built with productivity and performance in mind. 

Documentation for v2 is available to [view here](http://polylytics.github.io/dashing/). 

# Features

* Convention over Configuration with code-first minimal configuration
* Sql-like strongly typed query syntax
* Paging support
* Eager loading of relationships
* Change tracking
* Crud operations
* Default transactional behaviour
* Schema generation/migrations
* Dynamic method generation and caching
* Builds on top of Dapper 
* Multiple database support (SQL Server/MySql right now)
* In-memory engine for testing

# Examples

Get Entity

	var post = await session.GetAsync<Post>(123);
	var post = await session.Query<Post>().SingleAsync(p => p.PostId == 123);

Insert
	
	var post = new Post { Title = "Hello World" };
	await session.InsertAsync(post);
	Console.WriteLine(post.PostId); // 123

Update changed properties only

    var post = await session.GetAsync<Post>(123);
    post.Title = "New Title";
    await session.SaveAsync(post); // update [Posts] set [Title] = @P1 where [PostId] = @P2

Delete

	await session.DeleteAsync(post);

Eager fetching of related entities

    var posts = await session.Query<Post>()
				.Fetch(p => p.Author)
				.Fetch(p => p.Tags)
                .FetchMany(p => p.Comments).ThenFetch(c => c.Author)
                .Where(p => p.Category == ".Net ORM")
				.OrderByDescending(p => p.CreatedDate)
				.ToListAsync();

Paging

	var firstPage = await session.Query<Post>().AsPagedAsync(0, 10);

Count/Any

	var numberPosts = await session.Query<Post>().CountAsync(p => p.Author.UserId == userId);
	var hasAuthored = await session.Query<Post>().AnyAsync(p => p.Author.UserId == userId);

Bulk update entity

    await session.UpdateAsync<Post>(p => p.IsArchived = true, p => p.Author.UserId == 3);
    // update [Posts] set [IsArchived] = @P1 where [AuthorId] = @P2

Bulk delete

	await session.DeleteAsync<Post>(p => p.IsArchived);

Drop to Dapper

    await session.Dapper.QueryAsync("select 1 from Foo");

Inspect changes
	
	post.Title = "New";
	session.Inspect(post).IsPropertyDirty(p => p.Title);
	var oldTitle = session.Inspect(post).GetOldValue(p => p.Title); // Old

Migrate database to match latest code

    ./dash migrate -a "<path to assembly>" -t "<configuration type full name>" -c "<connection string>" 

# Who uses Dashing?

Dashing has been developed over the last 4 years at Polylytics and is in use at nearly all of our clients. It's used to execute millions of queries every week.

Feature requests (and voting for them) available at [Feathub](http://feathub.com/Polylytics/dashing)

[![Join the chat at https://gitter.im/Polylytics/dashing](https://badges.gitter.im/Polylytics/dashing.svg)](https://gitter.im/Polylytics/dashing?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) 

