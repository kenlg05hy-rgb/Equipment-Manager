# Test Medical Device API Endpoints
Write-Host "Starting API..." -ForegroundColor Cyan

$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"d:\Project\EquipmentManager\MedicalDeviceApi\MedicalDeviceApi.csproj`"" -WorkingDirectory "d:\Project\EquipmentManager" -PassThru -RedirectStandardOutput "api-output.log" -RedirectStandardError "api-error.log"

Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

try {
    # Test GET
    Write-Host "`n=== Testing GET /api/Devices ===" -ForegroundColor Green
    $getResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices" -Method Get
    Write-Host "Status: $($getResponse.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($getResponse.Content)" -ForegroundColor White

    # Test POST
    Write-Host "`n=== Testing POST /api/Devices ===" -ForegroundColor Green
    $postBody = @{
        DeviceName = "Test Device $(Get-Date -Format 'HHmmss')"
        SerialNumber = "SN-TEST-$(Get-Random -Maximum 9999)"
        Status = "Active"
    } | ConvertTo-Json

    $postResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices" -Method Post -Body $postBody -ContentType "application/json"
    Write-Host "Status: $($postResponse.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($postResponse.Content)" -ForegroundColor White

    # Test PUT
    Write-Host "`n=== Testing PUT /api/Devices/1 ===" -ForegroundColor Green
    $putBody = @{
        DeviceName = "Updated Device"
        Status = "Maintenance"
    } | ConvertTo-Json

    try {
        $putResponse = Invoke-WebRequest -Uri "http://localhost:5244/api/Devices/1" -Method Put -Body $putBody -ContentType "application/json"
        Write-Host "Status: $($putResponse.StatusCode)" -ForegroundColor Green
        Write-Host "Response: $($putResponse.Content)" -ForegroundColor White
    } catch {
        Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Yellow
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response: $($reader.ReadToEnd())" -ForegroundColor White
    }

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Write-Host "`nStopping API..." -ForegroundColor Cyan
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Done!" -ForegroundColor Green
}
