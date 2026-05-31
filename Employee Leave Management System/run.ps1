# LeaveFlow System Startup Script
# This script launches both the backend API and the Blazor client in separate windows.

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "STARTING LEAVEFLOW EMPLOYEE LEAVE SYSTEM" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Start the Backend API
Write-Host "Starting ASP.NET Core API on http://localhost:5220..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "$Host.UI.RawUI.WindowTitle='LeaveFlow API Backend'; cd backend; dotnet run --launch-profile http"

# 2. Start the Blazor Client
Write-Host "Starting Blazor Client on http://localhost:5027..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "$Host.UI.RawUI.WindowTitle='LeaveFlow Blazor Frontend'; cd frontend; dotnet run"

Write-Host "----------------------------------------------------------"
Write-Host "Startup commands dispatched. Please check the two new" -ForegroundColor Green
Write-Host "PowerShell windows for build and run outputs." -ForegroundColor Green
Write-Host "----------------------------------------------------------"
