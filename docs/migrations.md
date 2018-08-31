# Migrations

Migrations are done using the dash tool including in Dashing.Cli (see [Getting Started](start.html)) 
This tool lets you specify a database and the configuration and the tool will read the schema of
that database and then generate a script to upgrade it to the configuration you've specified.

To get help on running the dash tool simply run 

	(dotnet) dash --help
	
You'll see that there are 4 commands available within dash at the moment:

* addweave - see [start](Getting started) for information on this.
* script - generates a script that will update the specified database to match your IConfiguration
* migrate - will execute the script that is generated from script
* seed - will execute a function that lets you auto-populate data.

For help on using each function type

	(dotnet) dash <command> --help
	
> Note, that blindly running migrate on a production database properly isn't the best idea. We usualy do a script first, check it and then run migrate.