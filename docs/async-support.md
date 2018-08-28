We've designed Dashing so that all statements which hit the database (or could) make use
 of the new async-await support from C# 5.0.

However, if you need to keep your code synchronous then every method has a synchronous
 partner which is usually the same method signature but without the Async in the name e.g.

* GetAsync -> Get
* InsertAsync -> Insert
* SaveAsync -> Save
* DeleteAsync -> Delete

Dashing also supports the use of MARS (Multiple Active Result Sets) so that you can execute 
multiple queries concurrently and then await the results as you need them e.g.

	var commentsTask = session.Query<Comment>().Where(c => c.Post.PostId == 1).ToListAsync();
	var relatedPostsTask = session.Query<RelatedPost>().Fetch(rp => rp.RelatedPost).Where(rp => rp.Post.PostId == 1).ToListAsync();
	
	... // some other code
	
	var comments = await commentsTask;
	
	... // do some stuff with comments
	
	var relatedPosts = await relatedPostsTask;
	
Using the above you can easily queue all queries concurrently and then use the results when you need them.
In an ASP.NET application this should minimise the time taken to execute the whole page.