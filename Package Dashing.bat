@echo off
set /p version="What is the package version? "
dotnet pack ./Dashing/Dashing.csproj --output ./ --configuration Release /p:PackageVersion=%version%
dotnet pack ./Dashing.Cli/Dashing.Cli.csproj --output ./ --configuration Release /p:PackageVersion=%version%
dotnet pack ./Dashing.Cli/Dashing.Tool.csproj --output ./ --configuration Release /p:PackageVersion=%version%
dotnet pack ./Dashing.Weaver/Dashing.Weaver.csproj --output ./ --configuration Release /p:PackageVersion=%version%
pause