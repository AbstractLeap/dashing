---
queries: true
---

# Fetching associations

We've made a (semi-) conscious decision in Dashing to only enable fetching of relations through
 eager fetching - there is no support for Lazy fetching (we think it leads to lazy coding
 and unobvious performance issues such as n+1)

We have, however, made it rather simple to enable fetching of related entities...If 
we go back to the Blog example then we may have a simple domain like such:

Post

* PostId int
* Title string
* Blog Blog

Blog

* BlogId int
* Title string
* Posts IList<Post>

Default Fetching
----------------

By default if we ask Dashing for all of the Posts:

    var posts = await session.Query<Post>().ToArrayAsync();

we'll get back an array of Post entities which (assuming they have a Blog reference i.e. the BlogId
 value in the database is not null) have instances of Blog on them but the only thing these 
 instances have populated is the BlogId primary key. This is useful in it's own right as it means that
 you can perform updates to the Blog property without actually having to fetch the Blog columns as well
 as perform the join within the sql i.e. the BlogId column is on the Posts table and this lets us
 access it without having to do the join.

Many To One Fetching
-----------------

Now, if we do want to know the other properties of the Blog entity for each Post we'll want 
to fetch the Blog at the same time as we fetch the posts. This is easily achieved:

    var postsWithBlogs = await session.Query<Post>().Fetch(p => p.Blog).toArrayAsync();

Multiple Many To One Fetching
------------------------

Clearly there are going to be occasions where you would like to fetch an entity as well as
 multiple relations for that entity. Doing that is as simple as calling Fetch multiple times:

    var postsWithFetches = await session.Query<Post>()
                                        .Fetch(p => p.Blog.Owner)
                                        .Fetch(p => p.Blog.Country)
                                        .Fetch(p => p.Author)
                                        .ToArrayAsync();

You'll notice that the first fetch statement above actually goes and asks for a relationship
 on a relationship. Dashing simply traverses the relationships and returns both the columns
 for the Owner and the Blog. In the second fetch statement we ask for another relationship
 on a relationship but in this instance Dashing knows that it already has the Blog from the
 first fetch statement so only adds on the Country columns and the extra join. Note that,
 for fetched relationships, columns that are excluded by default will not be returned.

One to Many Fetching
-----------------------

One to many fetching in Dashing allows you to traverse collection type properties and
 fetch those entities as well. For example:

    var blogs = await session.Query<Blog>()
				 .Fetch(b => b.Posts)
				 .ToArrayAsync();

In this instance we simple fetch all of the blogs and all of their posts at the same
 time. If you would also like to fetch the Author of each Post then you can use the
 following type of query:

    var blogsWithPostsAndAuthors = await session.Query<Blog>()
									.FetchMany(b => b.Posts)
									.ThenFetch(p => p.Author)
									.ToArrayAsync();

As with many to one fetching you can traverse these trees multiple times and Dashing
 will only add the columns and joins for new entities in the tree.