param($installPath, $toolsPath, $package, $project)

Write-Host "Installing Dashing.Core"
Write-Host "Install Path: " -nonewline; Write-Host $installPath
Write-Host "Tools Path: " -nonewline; Write-Host $toolsPath
Write-Host "Package: " -nonewline; Write-Host $package
Write-Host "Project: " -nonewline; Write-Host $project


# find out where to put the files, we're going to create a deploy directory
# at the same level as the solution.

$rootDir = (Get-Item $installPath).parent.parent.fullname
$deployTarget = "$rootDir\Dashing"
Write-Host "Deploy Target: " -nonewline; Write-Host $deployTarget

# create our deploy support directory if it doesn't exist yet
if (!(Test-Path $deployTarget)) {
	mkdir $deployTarget
}

# copy everything in there
if (!(Test-Path $deployTarget\dev-db.ini)) {
    Write-Host "Copying dev-db.ini"
	Copy-Item $toolsPath\dev-db.ini $deployTarget -Recurse -Force
}

# don't need to do this as tools folder added to path?
# Do need to do this where the end user wants to overwrite the call to dbm as having to have package manager console open is a pita
Write-Host "Copying dbm.exe"
Copy-Item $toolsPath\dbm.exe $deployTarget -Recurse -Force

# get the active solution
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

# create a deploy solution folder if it doesn't exist

$deployFolder = $solution.Projects | where-object { $_.ProjectName -eq "Dashing" } | select -first 1

if(!$deployFolder) {
	$deployFolder = $solution.AddSolutionFolder("Dashing")

	# add all our support deploy scripts to our Support solution folder

	$folderItems = Get-Interface $deployFolder.ProjectItems ([EnvDTE.ProjectItems])

	ls $deployTarget | foreach-object { 
		$folderItems.AddFromFile($_.FullName) > $null
	} > $null
}