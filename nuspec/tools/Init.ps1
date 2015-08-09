param($installPath, $toolsPath, $package, $project)

# find out where to put the files, we're going to create a deploy directory
# at the same level as the solution.

$rootDir = (Get-Item $installPath).parent.parent.fullname
$deployTarget = "$rootDir\Dashing"

# create our deploy support directory if it doesn't exist yet
if (!(test-path $deployTarget)) {
	mkdir $deployTarget
}

# copy everything in there
if (!(Test-Path $toolsPath\dev-db.ini)) {
	Copy-Item $toolsPath\dev-db.ini $deployTarget -Recurse -Force
}

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