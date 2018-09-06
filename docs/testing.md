# Testing

When you're unit testing you'd generally like to avoid making actual database calls (for various reasons).

To that end we provide a simple way of mocking out all the SQL stuff in Dashing and replacing it with an in-memory version
of the database.

## What do you have to do?

Instead of using the `SqlDatabase` you can use the `InMemoryDatabase`. As long as all your code depends on ISession or IDatabase you can simply 
swap out these implementations during your tests. The majority of the Dashing api will work as expected when being used with in the InMemoryDatabase
 (transaction support is a notable omission) and using the `Dapper` property to execute SQL isn't going to work either.

> For example, you don't have to perform null-checking when run InMemory as the null checks are added for you. In the following code we don't need to check
that Author is not null in sql as the where clause is performed against the Users table (and the nulls propagate as expected). In order for that to work in memory
we re-write the lambda expression with null checks added.
		await session.Where(p => p.Author.IsApproved).ToArrayAsync()
		
!!! WARNING !!!

The implementation of InMemoryEngine, while not bad, may not exactly match what your database.