# Detailed API Test with Logging
Write-Host "Starting API with detailed logging..." -ForegroundColor Cyan

$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"d:\Project\EquipmentManager\MedicalDeviceApi\MedicalDeviceApi.csproj`"" -WorkingDirectory "d:\Project\EquipmentManager" -PassThru -NoNewWindow

Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

try {
    # Test OPTIONS to see available methods
    Write-Host "`n=== Testing OPTIONS /api/Devices/1 ===" -ForegroundColor Green
    try {
        $optionsResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices/1" -Method Options
        Write-Host "Status: $($optionsResponse.StatusCode)" -ForegroundColor Green
        Write-Host "Allow Header: $($optionsResponse.Headers['Allow'])" -ForegroundColor White
    } catch {
        Write-Host "OPTIONS not supported or failed" -ForegroundColor Yellow
    }

    # Get current devices to find a valid ID
    Write-Host "`n=== Getting current devices ===" -ForegroundColor Green
    $getResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices" -Method Get
    $devices = $getResponse.Content | ConvertFrom-Json
    Write-Host "Current devices:" -ForegroundColor White
    $devices | Format-Table -AutoSize
    
    if ($devices.Count -gt 0) {
        $deviceId = $devices[0].deviceID
        Write-Host "`n=== Testing PUT /api/Devices/$deviceId ===" -ForegroundColor Green
        
        $putBody = @{
            DeviceName = "Updated Device Name"
            Status = "Maintenance"
        } | ConvertTo-Json

        Write-Host "PUT URL: http://localhost:5244/api/Devices/$deviceId" -ForegroundColor Cyan
        Write-Host "PUT Body: $putBody" -ForegroundColor Cyan

        try {
            $putResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices/$deviceId" -Method Put -Body $putBody -ContentType "application/json"
            Write-Host "Status: $($putResponse.StatusCode)" -ForegroundColor Green
            Write-Host "Response: $($putResponse.Content)" -ForegroundColor White
        } catch {
            Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
            Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
            
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "No devices found in database" -ForegroundColor Yellow
    }

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
} finally {
    Write-Host "`nPress any key to stop API..." -ForegroundColor Cyan
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Done!" -ForegroundColor Green
}
