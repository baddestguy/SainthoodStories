# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host "This script requires PowerShell 7 or higher. Please run it in PowerShell 7 instead of double clicking on the script."
    Read-Host -Prompt "Press Enter to exit"
    exit
}


#Prompt user for game version
$versionName = Read-Host "Please enter the build version"

# Prompt user for console version
Write-Host "Choose an xbox console version:"
Write-Host "X1. For Xbox 1 and Xbox 1X|S Builds"
Write-Host "XS. For Xbox Series X|S Builds"

$console = Read-Host "Enter X1 or XS"
if ($console -notin @("X1", "XS")) {
    Write-Host "Invalid console choice. Exiting."; 
    Read-Host -Prompt "Press Enter to exit"
    exit 
}

$consoleVersionDirectory = Join-Path -Path $PSScriptRoot -ChildPath "$console/$versionName"
$packageDirectory = Join-Path -Path $consoleVersionDirectory -ChildPath "Package"
$looseDirectory = Join-Path -Path $consoleVersionDirectory -ChildPath "Loose"

# Check if the folder path exists
if (-not (Test-Path -Path $packageDirectory)) {
    
    Write-Host "**********************"
    $isNewBuild = Read-Host "The folder path $packageDirectory does not exist. Is this a new build? Y/N"
    
    if ($isNewBuild -ne "Y") {
        Write-Host "Build has not been created. Exiting..."
        Read-Host -Prompt "Press Enter to exit"
        exit
    }
    
    # Ensure Loose Folder is setup
    $parentDirectory = Split-Path -Path $PSScriptRoot -Parent;
    $sourceDirectory = Join-Path -Path $parentDirectory -ChildPath "Assets/_Scripts/Xbox/Sign-in/MicrosoftGameConfig/"
    $destinationDirectory = Join-Path -Path $PSScriptRoot -ChildPath "Loose"

    $pngFiles = Get-ChildItem -Path $sourceDirectory -Filter "*.png" -File

    # Copy each .png file to the destination directory
    foreach ($file in $pngFiles) {
        Copy-Item -Path $file.FullName -Destination $destinationDirectory
    }
    

    # copy destintion folder to loose directory
    Copy-Item -Path $destinationDirectory -Destination $looseDirectory -Recurse

    $shouldAutoComplete = "X";
    do {
        # Prompt user to go complete the build in Unity (another application)
        $shouldAutoComplete = Read-Host "Should I zip Files once build is complete? Y/N"
    }while ($shouldAutoComplete -notin @("Y", "N"))

    Write-Host "**********************"
    Write-Host "You are now ready to create the build. Please complete the build in Unity and return to this script to zip the files."
    Write-Host "**********************"

    # check every 10 seconds if package directory exists
    $totalWaitTime = 0;
    $waitInterval = 10;
    while (-not (Test-Path -Path $packageDirectory)) {
        Write-Host "Waiting $totalWaitTime seconds for package directory to be created..."
        Start-Sleep -Seconds $waitInterval
        $totalWaitTime += $waitInterval
    }

    
    if ($shouldAutoComplete -ne "Y") {
        Write-Host "**********************"
        Write-Host "Package directory found. Zipping files..."
        Write-Host "Exiting..."
        Read-Host -Prompt "Press Enter to exit"
        exit;
    }
}

Write-Host "**********************"
Write-Host "Zipping $versionName for $console"

$confirmChoice = Read-Host "Confirm Y/N?"
if ($confirmChoice -ne "Y") {
    Write-Host "Exiting..."
    Read-Host -Prompt "Press Enter to exit"
    exit;
}

# Define regex patterns for the files to match
$regexEscapedVersionName = [regex]::Escape($versionName);
$patterns = @(
    "^.*($regexEscapedVersionName).*zip$", # Example: TaiwoPicturesInc.Sainthood-TheGame_2.6.5.3_neutral__w2r0j31x2kfs0_x_60fcc671.zip
    "^.*($regexEscapedVersionName).*ekb$", # Example: TaiwoPicturesInc.Sainthood-TheGame_2.6.5.3_neutral__w2r0j31x2kfs0_x_Full_33ec8436-5a0e-4f0d-b1ce-3f29c3955039.ekb
    "^.*($regexEscapedVersionName).*xvc$", # Example: TaiwoPicturesInc.Sainthood-TheGame_2.6.5.3_neutral__w2r0j31x2kfs0_x.xvc
    "^Validator.*($regexEscapedVersionName).*xml$"  # Example: backup_daily.bak
)

# List to store matched files
$filesToZip = @()

# Search for files matching each pattern
foreach ($pattern in $patterns) {
    # Get the first file matching the regex pattern in the directory
    $file = Get-ChildItem -Path $packageDirectory -File | Where-Object { $_.Name -match $pattern } | Select-Object -First 1
    if ($file) {
        $filesToZip += $file
        Write-Host "Matched file: $($file.Name) for pattern '$pattern'"
    }
    else {
        Write-Host "No file found matching pattern '$pattern' in '$packageDirectory'"
    }
}

# Check if we found 4 files to zip
if ($filesToZip.Count -lt 4) {
    Write-Host "Less than 4 files matched. Exiting."
    Read-Host -Prompt "Press Enter to exit"
    exit
}

# Create the zip file path
$zipPath = Join-Path -Path $PSScriptRoot -ChildPath "$console-$versionName.zip"

# Warn if the zip already exists
if (Test-Path -Path $zipPath) {
    $overrideChoice = Read-Host "Zip already exists. Overwrite? Y/N"
    if ($overrideChoice -ne "Y") {
        Write-Host "Exiting..."
        Read-Host -Prompt "Press Enter to exit"
        exit;
    }

    Remove-Item -Path $zipPath
}

Write-Host "Creating zip file at $zipPath..."

# Create a new zip file and add matching files to it
# Add-Type -AssemblyName System.IO.Compression.FileSystem
$assemblyPath = "$([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory())\System.IO.Compression.FileSystem.dll"
Write-Host "Loading assembly from $assemblyPath..."
Add-Type -Path $assemblyPath

try {
    $zipFile = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
    foreach ($file in $filesToZip) {
        Write-Host "Adding $($file.Name) to $ZipFileName..."
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipFile, $file.FullName, $file.Name)
        Write-Host "Added $($file.Name) to $ZipFileName"
    }
    $zipFile.Dispose()

    Write-Host "Zip file created successfully at $zipPath"
}
catch {
    Write-Host "An error occurred: $_"
}

Read-Host -Prompt "Press Enter to exit"
