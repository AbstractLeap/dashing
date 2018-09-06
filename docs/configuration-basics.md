---
configuration: true
---

# Basics

Your IConfiguration provides information regarding which classes are in your domain and how those classes
are mapped to the database.

Assuming you're inheriting from BaseConfiguration you'll be able to access the
Add, AddNamespaceOf and Setup methods from within your constructor. These methods allow you to quickly 
add the domain classes. Note, that they will only be added once, irrespective of how many times the
method is called.

Setup<T>()
---------------

Add and AddNamespaceOf are very simple, just telling your Configuration that a class should be mapped 
by convention.

Setup, on the otherhand* allows you to modify the mapping. Setup<T>() returns the IMap<T> for the table
which then gives you access to the Columns, Indexes, Foreign Keys as well as Table specific properties
 (such as Name, Schema)

We suggest following intellisense to see what can be achieved but some common examples that we always
 seem to perform are:

### Setting Max Length on a String Column

    this.Setup<Feedback>().Property(f => f.Content).MaxLength();

### Ignoring a Property as it's not persisted to the database

    this.Setup<Foo>().Property(f => f.Total).Ignore();

### Adding an Index

    this.Setup<Person>().Index(a => new { a.EmailAddress });

### Adding a multi-column Index

    this.Setup<Person>().Index(a => new { a.FirstName, a.Surname });

### Adding a unique Index

    this.Setup<Person>().Index(a => new { a.EmailAddress }, true);

### Changing a table name

    this.Setup<Todo>().Table("Todos")

\* Setup calls Add internally