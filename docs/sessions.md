The IConfiguration, IDatabase and ISession interfaces are the main entry points for 
Dashing. Your IConfiguration details how your domain model is mapped to the database,
the IDatabase lets you perform sql queries by creating ISessions. 

There are pretty much two methods available for getting an ISession, both of which are
 defined on the IDatabase interface. They are BeginSession and BeginTransactionLessSession.
 BeginSession will generally wrap your connection in an IDbTransaction so that you can commit/rollback
 your work as you wish while BeginTransactionLessSession will simply open a connection to the database
 without a transaction.

BeginSession
------------------

If you use the parameterless BeginSession function Dashing will manage your connection
 and transaction for you - this means that when you make your first query on an ISession
 returned by BeginSession() Dashing will open a new IDbConnection and begin a new IDbTransaction.
 All subsequent queries on that session will use them. In order to commit your transaction
 you must call Complete() on the ISession, otherwise your transaction will be rolled back
 when it is disposed of (ISession implements IDisposable.)

In addition to the parameterless function there are two more overloads which allow
 you to provide the IDbConnection or IDbTransaction yourself so that you can then
 manage their lifetime explicitly.

An example of opening a session is:

	IDatabase database = ...;
	using (var session = database.BeginSession()) {
		var post = await session.GetAsync<Post>(1);
		session.Complete();
	}

Note that if you're developing a web application and are using an Inversion of Control
 container we'd suggest setting up your IConfiguration and IDatabase with a Singleton lifestyle
 and your ISession's with a lifestyle that equates to one per web request.

BeginTransactionLessSession
-----------------------

This function is similar to the above but does not use an IDbTransaction. As a result,
 most databases will automatically commit any changes for you. We would suggest that
 you stick with the BeginSession() method unless you particularly don't need an IDbTransaction.
 This can be useful if, for example, you are using a read only type database as
 it does result in a performance increase.

As with BeginSession it has an overload that allows you to pass in your own IDbConnection.