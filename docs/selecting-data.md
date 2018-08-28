Getting data out of your database is done using an [ISession](sessions) and, mostly,
 the methods named GetAsync and Query. GetAsync is used for fetching
entities by Id while Query is a more general purpose method allowing you to build up your
 queries and then fetch the results.

GetAsync
------------------

The GetAsync method has quite a few overloads but essentially allows you to fetch entities based on their id.
 There are a few extension methods providing access for common primary key datatypes such
 as int and Guid. The following code demonstrates the use.

    using (var session = database.BeginSession()) {
        var post = await session.GetAsync<Post>(1);
        var multiplePosts = await session.GetAsync<Post>(2, 3, 4);
    }

Query
-------------------

The Query function is your swiss army knife for getting data out of your database.
 It provides a fluent interface for performing SELECT queries and implements
 IEnumerable<T> so that you can use ToListAsync, ToArrayAsync, FirstAsync, LastAsync etc.
 It is not a fully fledged LINQ provider (it does not implement IQueryable) but does mirror some LINQ behaviour
 whilst also borrowing from NHibernate. We have designed it with simplicity
 in mind. For example, to get the last 10 blog posts you could do:

    var posts = await session.Query<Post>().OrderByDescending(p => p.Date).Take(10).ToListAsync();

### Where

Adding where clauses is pretty simple. You simply specify your where clause using a lambda expression e.g.

    var userPosts = await session.Query<Post>().Where(p => p.User.UserId == 1).ToListAsync();

 We have support for most types of expressions. You can, for example:

* use equals on entities e.g. `.Where(p => p.User == someUserObject)`
* do nested where clauses e.g. `.Where(p => p.Blog.Site.Title == "foo")`
* do arithmetic clauses e.g. `.Where(p => p.Rating > 3)`
* do string clauses e.g. `.Where(p => p.Title.Contains("foo"))`, `.Where(p => p.Title.StartsWith("foo"))`

Calling `Where()` twice on a query will generate sql with AND type behaviour

### OrderBy

You can add order by clauses to your queries and apply them to as many columns as you'd like

    var orderedPosts = await session.Query<Post>().OrderBy(p => p.User).OrderBy(p => p.Rating).ToListAsync();

You can also use OrderByDescending to order in the other direction.

### Take and Skip

Take and Skip allow you to return paged results or just the top N results from a query e.g.

    var top10Posts = await session.Query<Post>().Take(10).ToListAsync();

    var secondPageOfPosts = await session.Query<Post>().Take(10).Skip(10).ToListAsync();

### Fetch

Dashing (or us at least*) only believe in providing Eager loading of associations out of the box.
So, if you would like to access properties on related entities in your queries you must Fetch() them explicitly. 
This is described in more detail on the [Fetching Associations](fetching-associations) page.

### Including/Excluding columns

When you set up your configuration it is possible to define some columns as being excluded by
 default. This is useful where you have, for instance, a large text property that you do not
 need to show in list views so you'd prefer not to get it by default. The Include/Exclude 
functions allow you to determine this behaviour per query

### ForUpdate

Using the ForUpdate function will lock the rows returned by the query so that you can perform an isolated update. This is useful for ensuring that only a single transaction can happen at a time.

\* we may be convinced otherwise