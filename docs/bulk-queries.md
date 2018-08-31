---
queries: true
---

# Bulk queries

SQL gives you the ability to execute update and delete statements against a table with a where clause to 
specify which rows to update. Dashing lets you do that too!

Updates
------------

Updating a sub-set of rows at once:

    await session.UpdateAsync<Post>(
		p => p.IsArchived = true, // the Action<Post> that specifies the updated property values
		p => p.CreatedDate < DateTime.UtcNow); // the Expression<Func<Post, bool>> that specifies the where clause on the update statement

Updating all rows:

    await session.UpdateAllAsync<Post>(p => p.IsNew = false);

You can also update more than 1 thing at a time:

    await session.UpdateAsync<Post>(p => {
        p.IsNew = false;
        p.IsArchived = true;
    }, p => p.CreatedDate < DateTime.UtcNow);

You'll notice that we've separated out these two functions so that it's a little harder to accidentally update all of the rows in a table.

In both cases though it's worth understanding how this is implemented and what, therefore, are it's limitations.
The first parameter is of type Action<T>. We take this function and execute it, passing in an entity of that type.
The entity that is passed in simply looks for use of the setter on each property and, if used, will include
that column in the update statement with the associated value. This means that currently you can only set new 
values, you can't perform increments or similar.

NOTE: We are looking to implement ++, --, +=, -= (and maybe more) in the future and current thoughts will result in the Action<T>
being called multiple times. With this in mind you might want to ensure that your Action<T> has no side effects.

Deletes
------------

Deleting a sub-set of rows:

    await session.DeleteAsync<Post>(p => p.IsArchived);

Deleting all rows:

    await session.DeleteAllASync<Post>();