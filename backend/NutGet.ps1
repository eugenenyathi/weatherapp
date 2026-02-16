<#
.SYNOPSIS
    Installs NuGet packages using .NET CLI.
.DESCRIPTION
    This script installs packages via `dotnet add package` instead of relying on Visual Studio's Package Manager.
#>

function Install-NuGetPackages {
    $packages = @(
        @{ Name = "AutoMapper"; Version = "13.0.1" },
        @{ Name = "FluentValidation"; Version = "11.11.0" },
        @{ Name = "FluentValidation.AspNetCore"; Version = "11.3.0" },
        @{ Name = "Microsoft.AspNetCore.Identity.EntityFrameworkCore"; Version = "9.0.0" },
        @{ Name = "Microsoft.AspNetCore.OpenApi"; Version = "9.0.0" },
        @{ Name = "Microsoft.EntityFrameworkCore"; Version = "9.0.0" },
        @{ Name = "Microsoft.EntityFrameworkCore.Design"; Version = "9.0.0" },
        @{ Name = "Microsoft.EntityFrameworkCore.SqlServer"; Version = "9.0.0" },
        @{ Name = "Microsoft.EntityFrameworkCore.Tools"; Version = "9.0.0" },
        @{ Name = "Hangfire"; Version = "1.8.18" },
        @{ Name = "Hangfire.AspNetCore"; Version = "1.8.18" },
        @{ Name = "Hangfire.SqlServer"; Version = "1.8.18" }
    )

    Write-Host "Starting NuGet package installation via .NET CLI..." -ForegroundColor Cyan

    foreach ($package in $packages) {
        $packageName = $package.Name
        $packageVersion = $package.Version
        $prereleaseFlag = if ($package.Prerelease) { "--prerelease" } else { "" }

        try {
            Write-Host "Installing $packageName v$packageVersion..." -ForegroundColor Yellow
            dotnet add package $packageName --version $packageVersion $prereleaseFlag
            Write-Host "Successfully installed $packageName v$packageVersion" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to install $packageName v$packageVersion" -ForegroundColor Red
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }

    Write-Host "Package installation completed." -ForegroundColor Cyan
}

# Execute the installation
Install-NuGetPackages