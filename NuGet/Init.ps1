param($installPath, $toolsPath, $package, $project)

Write-Host "Installing Dashing"
Write-Host "Install Path: " -nonewline; Write-Host $installPath
Write-Host "Tools Path: " -nonewline; Write-Host $toolsPath
Write-Host "Package: " -nonewline; Write-Host $package
Write-Host "Project: " -nonewline; Write-Host $project


# find out where to put the files, we're going to create a deploy directory
# at the same level as the solution.

$rootDir = (Get-Item $installPath).parent.parent.fullname
$deployTarget = "$rootDir\Dashing"
Write-Host "Deploy Target: " -nonewline; Write-Host $deployTarget

$appConfigFileDir = [IO.Path]::Combine($installPath, 'tools/net46/')
$appConfigFile = [IO.Path]::Combine($installPath, 'tools/net46/dashing.exe.config')
$appConfig = New-Object XML
$appConfig.Load($appConfigFile)
$node = $appConfig.SelectSingleNode('configuration/appSettings/add[@key="AssemblySearchPaths"]')
$node.Attributes['value'].Value = $appConfigFileDir
$appConfig.Save($appConfigFile)

# create our deploy support directory if it doesn't exist yet
if (!(Test-Path $deployTarget)) {
	mkdir $deployTarget
}

Copy-Item $installPath\tools\net46\dashing.exe $deployTarget -Recurse -Force
Copy-Item $installPath\tools\net46\dashing.exe.config $deployTarget -Recurse -Force
