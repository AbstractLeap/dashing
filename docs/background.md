# Background

This page provides details on some of the implementation details for Dashing.

## How does `Query` work?

The `Query` method provides a strongly typed, simple way of executing sql queries through dapper. Lets look at 2 equivalent queries taken from the 
PerformanceTests code. These two pieces of code get a post and the author of that post in one query.

**Dapper**
```
return dapperConn.Query<Post, User, Post>(
	// Sql query
	@"select 
		t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], 
		t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] 
	from [Posts] as t 
		left join [Users] as t_1 
			on t.AuthorId = t_1.UserId 
	where ([PostId] = @l_1)",
	
	// Mapping function
	(p, u) => {
		p.Author = u;
		return p;
	},
	
	// Parameters
	new { l_1 = i },
	
	// Where to split the result set for the multi-mapping
	splitOn: "UserId").First();
```

**Dashing**
```
return session.Query<Post>().Fetch(p => p.Author).First(p => p.PostId == i);
```

You can immediately see that the Dashing version is simpler and more maintainable (due to being strongly typed and therefore refactor friendly). 
You might ask "how does it work" and what is the cost (in terms of speed) implication.

When Dashing executes the above query it is basically doing the dapper query above i.e. it generates the Sql query, it then generates the mapping 
function at runtime, specifies the SplitOn parameter and inserts the parameters. The mapping function is a little bit more complex as it sets up 
change tracking and does null checks, for example. It's then cached based on the "tree" described by the `Fetch`s in the query. The IL re-writing at
compilation time, the IL generation at runtime and the caching thereof is how we manage to keep the performance at an acceptable point. 

This ability to generate dapper queries in a strongly typed and consistent manner was really the starting requirement for Dashing. All the other 
functionality as grown from there as it has been used in mutiple production systems.