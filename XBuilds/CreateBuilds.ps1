
# ask user if build has beenc created
# $buildCreated = Read-Host "Has the build been created? (Y/N)"
# if ($buildCreated -ne "Y") {
#     Write-Host "Build has not been created. Exiting..."
#     exit
# }

# Hardcoded pattern for matching files
$Pattern = "*.txt" # Modify this pattern as needed (e.g., "*.log", "*.csv")

# Prompt user for the folder path
$versionName = Read-Host "Please enter the build version"

# Prompt user to choose a file pattern
Write-Host "Choose an xbox console version:"
Write-Host "X1. For Xbox 1 and Xbox 1X|S Builds"
Write-Host "XS. For Xbox Series X|S Builds"

# Read user input and set the file pattern based on their choice
$console = Read-Host "Enter X1 or XS"

if ($console -notin @("X1", "XS")) {
    Write-Host "Invalid console choice. Exiting."; 
    exit 
}

Write-Host "Zipping $versionName for $console"

$confirmChoice = Read-Host "Confirm Y/N?"
if ($confirmChoice -ne "Y") {
    Write-Host "Exiting..."
    exit;
}

$packageDirectory = Join-Path -Path $PSScriptRoot -ChildPath "$console/$versionName/Package"

# Check if the folder path exists
if (-not (Test-Path -Path $packageDirectory)) {
    Write-Host "The folder path $packageDirectory does not exist."
    exit
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
    exit
}

# Create the zip file path
$zipPath = Join-Path -Path $PSScriptRoot -ChildPath "$console-$versionName.zip"

# Warn if the zip already exists
if (Test-Path -Path $zipPath) {
    $overrideChoice = Read-Host "Zip already exists. Overwrite? Y/N"
    if ($overrideChoice -ne "Y") {
        Write-Host "Exiting..."
        exit;
    }

    Remove-Item -Path $zipPath
}

Write-Host "Creating zip file at $zipPath..."

# Create a new zip file and add matching files to it
Add-Type -AssemblyName System.IO.Compression.FileSystem

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
