This project only exists as a work around to the issue that MSBUILD (inside of VS) runs all tasks within the primary app domain.

This causes issues when developing Dashing as the standard MSBuild tasks that you'd use in a project (that references Dashing) can not be used as they effectively lock the files.

As a result, this console program performs the weaving necessary for Dashing.Tests, Dashing.Integration.Tests, Dashing.ElasticSearch.Tests, Dashing.Testing.Tests, Dashing.Tools.Tests and PerformanceTest

	-- maybe there's too many Test projects ???

There's a AFterBuild target added to the bottom of the .csproj files that calls this and performs the weaving. Any new projects that use IConfigurations should add that.