# Final PUT Endpoint Test
Write-Host "Stopping all API processes..." -ForegroundColor Cyan
Get-Process -Name "MedicalDeviceApi" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

Write-Host "Starting API..." -ForegroundColor Cyan
$app = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"d:\Project\EquipmentManager\MedicalDeviceApi\MedicalDeviceApi.csproj`"" -PassThru -NoNewWindow -WorkingDirectory "d:\Project\EquipmentManager"

Write-Host "Waiting for API startup (8 seconds)...`n" -ForegroundColor Yellow
Start-Sleep -Seconds 8

try {
    # Test 1: PUT with simple ASCII
    Write-Host "=== Test 1: PUT with ASCII ===" -ForegroundColor Green
    $json = '{"DeviceName":"Updated Device","Status":"Active"}'
    $response = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices/1" -Method Put -Body $json -ContentType "application/json"
    Write-Host "SUCCESS! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)`n" -ForegroundColor White
    
} catch {
    Write-Host "FAILED! Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)`n" -ForegroundColor Red
}

Write-Host "Stopping API..." -ForegroundColor Cyan
Stop-Process -Id $app.Id -Force
Write-Host "Done!" -ForegroundColor Green
