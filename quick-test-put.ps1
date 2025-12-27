# Quick PUT Test
Write-Host "Stopping any running API..." -ForegroundColor Cyan
Get-Process -Name "MedicalDeviceApi" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Write-Host "Starting API..." -ForegroundColor Cyan
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"d:\Project\EquipmentManager\MedicalDeviceApi\MedicalDeviceApi.csproj`"" -WorkingDirectory "d:\Project\EquipmentManager" -PassThru -NoNewWindow

Write-Host "Waiting 8 seconds for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

try {
    # Get devices
    Write-Host "`n=== GET Devices ===" -ForegroundColor Green
    $devices = Invoke-RestMethod -Uri "http://localhost:5244/api/Devices" -Method Get
    Write-Host "Found $($devices.Count) devices" -ForegroundColor White
    $devices | Select-Object -First 3 | Format-Table

    # Test PUT with first device
    if ($devices.Count -gt 0) {
        $deviceId = $devices[0].deviceID
        $originalName = $devices[0].deviceName
        
        Write-Host "=== PUT Device ID: $deviceId ===" -ForegroundColor Green
        
        $putBody = "{`"DeviceName`": `"UPDATED: $originalName`", `"Status`": `"Under Maintenance`"}"

        Write-Host "Request: PUT http://localhost:5244/api/Devices/$deviceId" -ForegroundColor Cyan
        Write-Host "Body: $putBody`n" -ForegroundColor Cyan

        $response = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices/$deviceId" -Method Put -Body $putBody -ContentType "application/json"
        
        Write-Host "SUCCESS!" -ForegroundColor Green
        Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "Response: $($response.Content)" -ForegroundColor White

        # Verify the update
        Write-Host "`n=== Verifying Update ===" -ForegroundColor Green
        $updatedDevices = Invoke-RestMethod -Uri "http://localhost:5244/api/Devices" -Method Get
        $updatedDevice = $updatedDevices | Where-Object { $_.deviceID -eq $deviceId }
        $updatedDevice | Format-List
    }
} catch {
    Write-Host "`nERROR!" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Body: $($reader.ReadToEnd())" -ForegroundColor Yellow
    }
    Write-Host "`nException Details:" -ForegroundColor Yellow
    Write-Host $_.Exception | Format-List -Force
} finally {
    Write-Host "`nStopping API..." -ForegroundColor Cyan
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    Write-Host "Done!" -ForegroundColor Green
}
