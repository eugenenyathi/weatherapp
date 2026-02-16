# Configuration
$BaseDirectory = "C:\Users\eugen\RiderProjects\weatherapp\weatherapp" # Adjust to your project's base directory
$ProjectFile = "C:\Users\eugen\RiderProjects\weatherapp\weatherapp\weatherapp.csproj" # Adjust to your .csproj file path
$Directories = @("Controllers", "Data", "DataTransferObjects", "Enums", "Entities", "Exceptions", "Mappers", "Middleware", "Requests", "Services", "Utils", "Validators") # Add or remove directories as needed

# Function to create directories
function Create-Directories {
    param (
        [string]$BasePath,
        [string[]]$DirectoryList
    )

    # Resolve the base path to ensure it's correct
    $ResolvedBasePath = Resolve-Path -Path $BasePath -ErrorAction SilentlyContinue
    if (-not $ResolvedBasePath) {
        Write-Error "Base directory '$BasePath' not found."
        return # Exit the function
    }
    $BasePath = $ResolvedBasePath.Path

    foreach ($dir in $DirectoryList) {
        $fullPath = Join-Path -Path $BasePath -ChildPath $dir
        if (-not (Test-Path -Path $fullPath)) {
            Write-Host "Creating directory: $fullPath"
            try {
                New-Item -ItemType Directory -Path $fullPath -ErrorAction Stop | Out-Null
            }
            catch {
                Write-Error "Failed to create directory '$fullPath': $($_.Exception.Message)"
            }
        }
        else {
            Write-Host "Directory already exists: $fullPath"
        }
    }
}

# Function to add ItemGroup to .csproj file
function Add-ItemGroupToProjectFile {
    param (
        [string]$ProjectPath,
        [string[]]$DirectoryList
    )

    $itemGroupXml = "<ItemGroup>`n"
    foreach ($dir in $DirectoryList) {
        $itemGroupXml += "  <Folder Include=`"$dir\`" />`n"
    }
    $itemGroupXml += "</ItemGroup>"

    try {
        $projectContent = Get-Content -Path $ProjectPath -Raw
        $projectContent = $projectContent.Replace("</Project>", "$itemGroupXml`n</Project>")
        Set-Content -Path $ProjectPath -Value $projectContent
        Write-Host "ItemGroup added to $ProjectPath"
    }
    catch {
        Write-Error "Failed to modify $ProjectPath : $($_.Exception.Message)"
    }
}

# Main script execution
if (-not (Test-Path -Path $BaseDirectory)) {
    write-host "Base directory does not exist, creating it now"
    try {
        New-Item -ItemType Directory -Path $BaseDirectory -ErrorAction Stop | Out-Null
    }
    catch {
        Write-Error "Failed to create base directory '$BaseDirectory': $($_.Exception.Message)"
        return
    }
}
Create-Directories -BasePath $BaseDirectory -DirectoryList $Directories
Add-ItemGroupToProjectFile -ProjectPath $ProjectFile -DirectoryList $Directories

Write-Host "Boilerplate generation and .csproj modification complete."