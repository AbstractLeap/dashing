---
queries: true
---

# Manipulating Data

Dashing attempts to provide an intuitive api for performing most of the common sql functions that you would expect to be able to perform 
e.g. selecting data as objects from the database, updating or deleting that data, inserting new data. 

We like to think of Dashing as a tool for easily performing sql functions, as opposed to a complete abstraction of the database, 
and as such you can also perform bulk updates to your database.

The following sections detail how to perform the various functions:

* [Sessions](sessions) - how to set up a connection to the database and manage transactions
* [Getting data out](selecting-data) - how to SELECT data
* [Saving and deleting entities](saving-and-deleting) - how to INSERT, UPDATE, DELETE
* [Fetching associations](fetching-associations) - how to eagerly load related entities
* [Bulk queries](bulk-queries) - how to perform bulk updates/deletes
* [Dapper](dapper) - how to revert to using Dapper
* [Async/sync support](async-support) - async and sync apis
* [Performance](performance) - how does Dashing perform?
