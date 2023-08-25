#########################################################
# RunLocalDockerLinuxServer.ps1
# Instead of deploying, we'll run it in a local container
#
# REQUIREMENTS:
# 1. Docker-Desktop
# 2. Use `HathoraServerConfig` to create a Unity build
# 3. Use `HathoraServerConfig` to start a deploy
#    (!) This only needs to create the .tar.gz @ `src/.hathora`;
#        It's ok if the deployment is !successful
#########################################################

$PathToBuild = "../src/.hathora"
$BuildNameNoExt = "Hathora-Unity_LinuxServer"
$BuildName = "$BuildNameNoExt.x86_64.tar.gz"
$NewContainerDir = "$PathToBuild\LocalContainer"
$Port = 7777

#########################################################

try {
	# Save current location
	$StartingDir = Get-Location

	# Init
	cls
	Write-Host "Starting..."

	# Validate paths
	if (!(Test-Path -Path $PathToBuild)) {
	  Write-Error "PathToBuild does not exist: $PathToBuild"
	  return
	}

	if (!(Test-Path -Path "$PathToBuild/$BuildName")) {
	  Write-Error "Build file does not exist: $PathToBuild/$BuildName"
	  return
	}

	# Check if directory exists
	if(!(Test-Path -Path $NewContainerDir )) {
	  # Create directory if it doesn't exist
	  New-Item -ItemType directory -Path $NewContainerDir
	}

	Write-Host "Preparing files in LocalContainer..."

	# Extract the tar file to the directory
	tar -xf "$PathToBuild/$BuildName" -C $NewContainerDir

	Write-Host "Files prepared. Building Docker image..."

	# Change to the directory
	Set-Location -Path $NewContainerDir

	# Build Docker image
	$ImageName="hathora-unity-server"
	docker build -t $ImageName .

	Write-Host "Image built. Running Docker container..."

	# Return to the original directory
	Set-Location -Path $StartingDir

	# Run Docker container, after 1st removing the old
	Write-Host "Attempting to run Docker container at localhost:$Port ..."
	docker rm -f "$BuildNameNoExt" 2>$null
	docker run --name "$BuildNameNoExt" -p "${Port}:${Port}" "$ImageName"
}
catch {
	Write-Error "An error occurred: $_"
    exit 1
}
