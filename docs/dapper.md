---
queries: true
---

# Dapper

There are occasions where you will want to use Dapper on its own so that you can execute a complicated query or simply maximise
 the performance of your database interaction. To take advantage of dapper simply
 use the Dapper property on your ISession and then invoke whichever method you need.
 This Dapper property, by default, makes use of the open IDbConnection and IDbTransaction
 in the session so that you remain in that transaction scope.

	using (var session = database.BeginSession()) {
		var stuff = await session.Dapper.QueryAsync<Stuff>("select * from stuff where id = @Id", new { Id = 1 });
	}