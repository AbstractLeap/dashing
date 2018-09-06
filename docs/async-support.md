---
queries: true
---

# Async/sync support

We've designed Dashing so that all statements which hit the database (or could) make use
 of the async-await support from C# 5.0.

However, if you need to keep your code synchronous then every method has a synchronous
 partner which is usually the same method signature but without the Async in the name e.g.

* GetAsync -> Get
* InsertAsync -> Insert
* SaveAsync -> Save
* DeleteAsync -> Delete

Dashing also supports the use of MARS ([Multiple Active Result Sets](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/multiple-active-result-sets-mars)) so that you can execute 
multiple queries concurrently and then await the results as you need them e.g.

	var commentsTask = session.Query<Comment>().Where(c => c.Post.PostId == 1).ToListAsync(); // sends query to the database, immediately continues here
	var relatedPostsTask = session.Query<RelatedPost>().Fetch(rp => rp.RelatedPost).Where(rp => rp.Post.PostId == 1).ToListAsync(); // as above
	
	... // some other code
	
	var comments = await commentsTask; // waits for the comments database query to return
	
	... // do some stuff with comments
	
	var relatedPosts = await relatedPostsTask; // waits for the related posts query to return
	
Using the above you can easily queue all queries concurrently and then use the results when you need them.
In an ASP.NET application this should minimise the time taken to execute the whole page.