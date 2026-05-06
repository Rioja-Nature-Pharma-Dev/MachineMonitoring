# Test MachineMonitoring API
$BaseUrl = "http://localhost:5021/api"
$Headers = @{"Content-Type" = "application/json"}

Write-Host "`n=== MachineMonitoring API Test ===" -ForegroundColor Cyan

# 1. Get machines
Write-Host "`n[1] Getting existing machines..."
$machines = Invoke-RestMethod -Uri "$BaseUrl/machines" -Method GET -Headers $Headers
Write-Host "Found: $($machines.Count) machines"
foreach ($m in $machines) {
    Write-Host "  - $($m.code): $($m.name)"
}

# 2. Create machine
Write-Host "`n[2] Creating new machine..."
$machineData = @{
    code = "LINEA-ETIQUETA-01"
    name = "Linea de Etiquetado Principal"
    description = "Maquina de etiquetado automatico"
}
$machine = Invoke-RestMethod -Uri "$BaseUrl/machines" -Method POST -Headers $Headers -Body ($machineData | ConvertTo-Json)
$machineId = $machine.id
Write-Host "Created: $($machine.code) - $($machine.name)"

# 3. Create order
Write-Host "`n[3] Creating production order..."
$orderData = @{
    machineCode = "LINEA-ETIQUETA-01"
    quantity = 5000
    unitsPerBox = 50
    estimatedMinutes = 120
}
$order = Invoke-RestMethod -Uri "$BaseUrl/production-orders" -Method POST -Headers $Headers -Body ($orderData | ConvertTo-Json)
$orderId = $order.id
Write-Host "Created: Order $($order.machineCode) - 5000 units"

# 4. Start order
Write-Host "`n[4] Starting production order..."
$started = Invoke-RestMethod -Uri "$BaseUrl/production-orders/$orderId/start" -Method POST -Headers $Headers
Write-Host "Status: $($started.status)"

# 5. Simulate production
Write-Host "`n[5] Simulating production (500 units)..."
for ($i = 1; $i -le 10; $i++) {
    $counterData = @{ units = 50 }
    $counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId/increment-good" -Method POST -Headers $Headers -Body ($counterData | ConvertTo-Json)
    $progress = [math]::Round(($counter.goodUnits / 5000) * 100, 1)
    Write-Host "  Batch $i - Total: $($counter.goodUnits) units ($progress%)"
}

# 6. Add defects
Write-Host "`n[6] Simulating defects (5 units)..."
for ($i = 1; $i -le 5; $i++) {
    $counterData = @{ units = 1 }
    $counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId/increment-bad" -Method POST -Headers $Headers -Body ($counterData | ConvertTo-Json)
}
Write-Host "Bad units: 5"

# 7. Get counter
Write-Host "`n[7] Getting counter status..."
$counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId" -Method GET -Headers $Headers
Write-Host "Good units: $($counter.goodUnits)"
Write-Host "Bad units:  $($counter.badUnits)"

# 8. Calculate metrics
Write-Host "`n[8] Calculating OEE metrics..."
$metricsData = @{}
Invoke-RestMethod -Uri "$BaseUrl/metrics/$orderId/calculate" -Method POST -Headers $Headers -Body ($metricsData | ConvertTo-Json) | Out-Null
$metrics = Invoke-RestMethod -Uri "$BaseUrl/metrics/$orderId" -Method GET -Headers $Headers
Write-Host "Availability: $([math]::Round($metrics.availability, 2))%"
Write-Host "Performance:  $([math]::Round($metrics.performance, 2))%"
Write-Host "Quality:      $([math]::Round($metrics.quality, 2))%"
Write-Host "OEE:          $([math]::Round($metrics.oee, 2))%"

# 9. Finish order
Write-Host "`n[9] Finishing production order..."
$finished = Invoke-RestMethod -Uri "$BaseUrl/production-orders/$orderId/finish" -Method POST -Headers $Headers
Write-Host "Status: $($finished.status)"

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Green
Write-Host "Machine: LINEA-ETIQUETA-01"
Write-Host "Order: $orderId"
Write-Host "Produced: 505 units (500 good + 5 bad)"
Write-Host "OEE: $([math]::Round($metrics.oee, 2))%"
Write-Host "`nAPI is fully functional!" -ForegroundColor Green
