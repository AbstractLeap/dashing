# Getting Started

Installing Dashing is done via Nuget. There are 3 libraries that you will need:

- **Dashing** <https://www.nuget.org/packages/Dashing>  
 The core library, it contains all of the code that you will use to query the database.
- **Dashing.Weaver** <https://www.nuget.org/packages/Dashing.Weaver>  
 A tool library that is used during the compilation of your projects to inject extra behaviour in to your domain model.
- **dotnet-dashing** (**Dashing.Cli** for .Net Framework) <https://www.nuget.org/packages/dotnet-dashing>  
 A library that provides a console application for performing updates to your databases e.g. schema migrations as you change your domain model.

In many of the projects that we run we have multiple applications accessing the same database through Dashing. 
As a result our standard setup is to have a separate "class library" project (e.g. MyProject.Domain) which will contain your 
domain model. 

Installation then depends on whether you're using .Net Framework or .Net Core:

### .Net Framework

```
Install-Package Dashing
Install-Package Dashing.Weaver
Install-Package Dashing.Cli
```

### .Net Core

```
dotnet add package Dashing
dotnet add package Dashing.Weaver
```

The `dotnet-dashing` package is installed as a .Net tool (either globally or locally)

```
dotnet tool install -g dotnet-dashing
```

## Your Domain Model

Once you've done this you can create the first version of your domain model using POCOs (Plain old CLR objects). For example:

```
public class Blog {
	public int BlogId { get;set; }

	public string Title { get; set; }

	public IList<Post> Posts { get; set; }
}

public class Post {
	public int PostId { get; set; }

	public string Title	{ get; set; }

	public DateTime Date { get; set; }

	public Blog Blog { get; set; }
}
```

A couple of things to note, which may be different from other ORMs:

1. At the moment we only support single column primary keys and by default they must be named `<ClassName>Id` or `Id`. You can override this if you want but there are many [conventions](configuration-conventions) to prevent [RSI](https://en.wikipedia.org/wiki/Repetitive_strain_injury) inducing activities.
2. The foreign key columns (e.g. `Blog` on `Post`) are strongly typed.
3. We don't need to make things virtual. Many ORMs use proxies to inject behaviour in to the domain instances, which sometimes requires properties to be virtual, however we re-write the IL at compile time. Beta versions of Dashing did actually use proxy technology but we got bored of typing or forgetting it! 

## The Configuration

How your domain model is mapped in to a database schema is done via a Configuration object. In order to do this create a class that inherits from `Dashing.Configuration.BaseConfiguration`. For example:

```
public class DashingConfiguration : BaseConfiguration {
    public DashingConfiguration() {
        this.AddNamespaceOf<Blog>();
    }
}
```

You can override many things in the [configuration](configuration) and do so either by providing a new [convention](configuration-conventions) or by overriding each class/property.

## Injecting the Compilation Step

As mentioned above, we've taken the approach of injecting behaviour in to the POCOs during compilation as opposed to at runtime. 
The Dashing.Weaver library takes care of this for us but we need to tell it which classes form part of our domain model.

> Fans of version 1 of Dashing may be wondering why they didn't do this before. Well, in version 1 we tried to automatically discover all configurations for you. While this was lovely it did increase build times rather unnecessarily (and a lot). So, given you do it approximately once per project it's now a manual step.

So, we now turn to the console application included in the Dashing.Cli nuget package. Depending on your platform this can be accessed in different ways:

___

### .Net Framework

A folder named "Dashing" should have been created at the root of your solution. In this folder is an application called "dash". You invoke it the same way you would any other console application.

### .Net Core

If you're using the dotnet cli tools (i.e. you type things like `dotnet new` at the command line) the dash tool gets installed as something you can call in the same way. So you just call `dotnet dash <command>` from within the solution directory.

___

So, simply open your shell of choice and type:

    [dotnet] dash addweave 
        -p "<path to csproj file of domain project>" 
        -c "<configuration type full name>" 
        -a "<the extension of the assembly produced by your domain model>"

As an example:

    dotnet dash addweave -p ".\MyProject.Domain\MyProject.Domain.csproj" -c "MyProject.Domain.DashingConfiguration" -a "dll"

This modifies your csproj slightly by adding the following section. This signals to Dashing.Weaver what configuration it should use in order to identify the classes that it should modify:

    <PropertyGroup>
        <WeaveArguments>-p "$(MSBuildThisFileDirectory)$(OutputPath)$(AssemblyName).dll" -t  "MyProject.Domain.DashingConfiguration"</WeaveArguments>
    </PropertyGroup>
	
In .Net Core it also modifies your csproj file slightly by splitting out the <Project> node in to separate props and targets imports, so that OutputPath is set correctly.

Build your solution and make sure everything's good to go! 

> At this point you can go open MyProject.Domain.dll (from you bin directory) using an Il Decompiler (maybe [IlSpy](https://github.com/icsharpcode/ILSpy)) and see the changes that have been made to your POCOs.

## Creating your Database

Now you have your domain model, configuration and a built project you can use dash again to create the database and schema for your application. In your command prompt again type:

    [dotnet] dash migrate
        -a "<path to the assembly containing your configuration>"
        -t "<configuration type full name>"
        -c "<connection string>"

As an example:

    dotnet dash migrate -a ".\MyProject.Domain\bin\Debug\MyProject.Domain.dll" -t "MyProject.Domain.DashingConfiguration" -c "Data Source=.;Initial Catalog=dbname;Integrated Security=True;"

You should now have your database created.

## Access your Database

In order to query the database you need a connection to it. We do that using an instance of an `IDatabase`. Dashing comes with 2 implementations of `IDatabase`: `SqlDatabase` and `InMemoryDatabase`. The latter is used for [testing](testing) purposes whilst the former lets you access your newly created database.

```
var dashingConfig = new DashingConfiguration();
var database = new SqlDatabase(dashingConfig, "<connection string>");
using (var session = database.BeginSession()) {
    var post = await session.GetAsync<Post>(1);
    post.Title = "Whoop";
    await session.SaveAsync(post);
    session.Complete();
}
``` 

In general we'd expect each application that's using the domain model to have singleton instances of `DashingConfiguration` and `SqlDatabase` and to then begin sessions (using `BeginSession()`) whenever needed. The parameterless `BeginSession()` creates a connection and a transaction* which is automatically rolled back upon disposal unless `Complete()` is called on it. In web applications we generally have a single session per web-request.

\* Actually it doesn't create a transaction, or open the connection, until you do something with the session.