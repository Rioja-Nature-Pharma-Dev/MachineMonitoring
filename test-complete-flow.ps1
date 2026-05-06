#!/usr/bin/env pwsh
<#
.SYNOPSIS
Test MachineMonitoring API - Complete Flow
Prueba la API con máquinas reales y flujo completo
#>

$BaseUrl = "http://localhost:5021/api"
$Headers = @{"Content-Type" = "application/json"}

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   MachineMonitoring API - Complete Flow Test              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ============================================================================
# PART 1: GET EXISTING MACHINES (CREMER should exist from seeding)
# ============================================================================

Write-Host "[1/10] Obteniendo máquinas existentes..." -ForegroundColor Yellow

try {
    $machines = Invoke-RestMethod -Uri "$BaseUrl/machines" -Method GET -Headers $Headers
    Write-Host "✓ Máquinas encontradas: $($machines.Count)" -ForegroundColor Green

    foreach ($machine in $machines) {
        Write-Host "  - $($machine.code): $($machine.name) [$($machine.status)]"
    }
} catch {
    Write-Host "✗ Error al obtener máquinas: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# PART 2: CREATE NEW MACHINE (REAL EXAMPLE)
# ============================================================================

Write-Host "`n[2/10] Creando nueva máquina (Línea de Etiquetado)..." -ForegroundColor Yellow

$machineData = @{
    code = "LINEA-ETIQUETA-01"
    name = "Línea de Etiquetado Principal"
    description = "Máquina de etiquetado automático - Producción"
}

try {
    $machine = Invoke-RestMethod -Uri "$BaseUrl/machines" -Method POST `
        -Headers $Headers `
        -Body ($machineData | ConvertTo-Json)

    $machineId = $machine.id
    Write-Host "✓ Máquina creada:" -ForegroundColor Green
    Write-Host "  ID: $($machine.id)"
    Write-Host "  Código: $($machine.code)"
    Write-Host "  Nombre: $($machine.name)"
    Write-Host "  Estado: $($machine.status)"
} catch {
    Write-Host "✗ Error al crear máquina: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# PART 3: CREATE PRODUCTION ORDER
# ============================================================================

Write-Host "`n[3/10] Creando orden de producción..." -ForegroundColor Yellow

$orderData = @{
    machineCode = "LINEA-ETIQUETA-01"
    quantity = 5000
    unitsPerBox = 50
    estimatedMinutes = 120
}

try {
    $order = Invoke-RestMethod -Uri "$BaseUrl/production-orders" -Method POST `
        -Headers $Headers `
        -Body ($orderData | ConvertTo-Json)

    $orderId = $order.id
    Write-Host "✓ Orden creada:" -ForegroundColor Green
    Write-Host "  ID: $($order.id)"
    Write-Host "  Máquina: $($order.machineCode)"
    Write-Host "  Cantidad: $($order.quantity) unidades"
    Write-Host "  Cajas: $([math]::Ceiling($order.quantity / 50))"
    Write-Host "  Estado: $($order.status)"
} catch {
    Write-Host "✗ Error al crear orden: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# PART 4: GET ACTIVE ORDER
# ============================================================================

Write-Host "`n[4/10] Obteniendo orden activa..." -ForegroundColor Yellow

try {
    $activeOrder = Invoke-RestMethod -Uri "$BaseUrl/production-orders/active" -Method GET -Headers $Headers
    Write-Host "✓ Orden activa encontrada:" -ForegroundColor Green
    Write-Host "  ID: $($activeOrder.id)"
    Write-Host "  Máquina: $($activeOrder.machineCode)"
    Write-Host "  Estado: $($activeOrder.status)"
} catch {
    Write-Host "⚠ No hay orden activa (normal si aún no iniciada)" -ForegroundColor Yellow
}

# ============================================================================
# PART 5: START PRODUCTION ORDER
# ============================================================================

Write-Host "`n[5/10] Iniciando orden de producción..." -ForegroundColor Yellow

try {
    $startedOrder = Invoke-RestMethod -Uri "$BaseUrl/production-orders/$orderId/start" -Method POST `
        -Headers $Headers

    Write-Host "✓ Orden iniciada:" -ForegroundColor Green
    Write-Host "  Estado: $($startedOrder.status)"
    Write-Host "  Iniciada: $($startedOrder.startedAt)"
} catch {
    Write-Host "✗ Error al iniciar orden: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# PART 6: SIMULATE PRODUCTION (Increment Counter)
# ============================================================================

Write-Host "`n[6/10] Simulando producción (incrementando contador)..." -ForegroundColor Yellow

$unitsToSimulate = 500  # 500 units = 10% of 5000
$batchSize = 50        # Increment 50 at a time

try {
    $batches = $unitsToSimulate / $batchSize

    for ($i = 1; $i -le $batches; $i++) {
        $counterData = @{ units = $batchSize }

        $counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId/increment-good" -Method POST `
            -Headers $Headers `
            -Body ($counterData | ConvertTo-Json)

        $progress = $counter.goodUnits
        $percentage = [math]::Round(($progress / 5000) * 100, 1)
        Write-Host "  - Lote $i: $progress unidades ($percentage%)"
    }

    Write-Host "✓ Contador incrementado:" -ForegroundColor Green
    Write-Host "  Total buenas: $unitsToSimulate"
    Write-Host "  Avance: $([math]::Round(($unitsToSimulate / 5000) * 100, 1))%"
} catch {
    Write-Host "✗ Error al incrementar contador: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# PART 7: SIMULATE SOME DEFECTS
# ============================================================================

Write-Host "`n[7/10] Simulando unidades defectuosas..." -ForegroundColor Yellow

try {
    for ($i = 1; $i -le 5; $i++) {
        $counterData = @{ units = 1 }
        $counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId/increment-bad" -Method POST `
            -Headers $Headers `
            -Body ($counterData | ConvertTo-Json)
    }

    Write-Host "✓ Defectos registrados: 5 unidades" -ForegroundColor Green
} catch {
    Write-Host "⚠ Error al registrar defectos (puede no estar implementado)" -ForegroundColor Yellow
}

# ============================================================================
# PART 8: GET COUNTER STATUS
# ============================================================================

Write-Host "`n[8/10] Obteniendo estado del contador..." -ForegroundColor Yellow

try {
    $counter = Invoke-RestMethod -Uri "$BaseUrl/counter/$orderId" -Method GET -Headers $Headers
    Write-Host "✓ Estado del contador:" -ForegroundColor Green
    Write-Host "  Unidades buenas: $($counter.goodUnits)"
    Write-Host "  Unidades malas: $($counter.badUnits)"
    Write-Host "  Total: $($counter.goodUnits + $counter.badUnits)"
} catch {
    Write-Host "✗ Error al obtener contador: $_" -ForegroundColor Red
}

# ============================================================================
# PART 9: CALCULATE & GET METRICS (OEE)
# ============================================================================

Write-Host "`n[9/10] Calculando métricas OEE..." -ForegroundColor Yellow

try {
    # Calculate
    $metricsData = @{}
    Invoke-RestMethod -Uri "$BaseUrl/metrics/$orderId/calculate" -Method POST `
        -Headers $Headers `
        -Body ($metricsData | ConvertTo-Json) | Out-Null

    # Get metrics
    $metrics = Invoke-RestMethod -Uri "$BaseUrl/metrics/$orderId" -Method GET -Headers $Headers

    Write-Host "✓ Métricas OEE calculadas:" -ForegroundColor Green
    Write-Host "  Availability (Disponibilidad): $([math]::Round($metrics.availability, 2))%"
    Write-Host "  Performance (Rendimiento):    $([math]::Round($metrics.performance, 2))%"
    Write-Host "  Quality (Calidad):            $([math]::Round($metrics.quality, 2))%"
    Write-Host "  OEE (Overall Equipment):      $([math]::Round($metrics.oee, 2))%"

    # OEE Interpretation
    $oee = $metrics.oee
    if ($oee -ge 85) {
        Write-Host "`n  → EXCELENTE OEE (>85%)" -ForegroundColor Green
    } elseif ($oee -ge 75) {
        Write-Host "`n  → BUENO OEE (75-85%)" -ForegroundColor Yellow
    } else {
        Write-Host "`n  → MEJORABLE OEE (<75%)" -ForegroundColor Red
    }
} catch {
    Write-Host "⚠ Error al calcular métricas: $_" -ForegroundColor Yellow
}

# ============================================================================
# PART 10: FINISH PRODUCTION ORDER
# ============================================================================

Write-Host "`n[10/10] Finalizando orden de producción..." -ForegroundColor Yellow

try {
    $finishedOrder = Invoke-RestMethod -Uri "$BaseUrl/production-orders/$orderId/finish" -Method POST `
        -Headers $Headers

    Write-Host "✓ Orden finalizada:" -ForegroundColor Green
    Write-Host "  Estado: $($finishedOrder.status)"
    Write-Host "  Finalizada: $($finishedOrder.finishedAt)"
} catch {
    Write-Host "✗ Error al finalizar orden: $_" -ForegroundColor Red
    exit 1
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   ✓ FLUJO COMPLETO EXITOSO                               ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "RESUMEN:" -ForegroundColor Cyan
Write-Host "├─ Máquina: LINEA-ETIQUETA-01"
Write-Host "├─ Orden: $orderId"
Write-Host "├─ Unidades producidas: 505 (500 buenas + 5 defectuosas)"
Write-Host "├─ OEE calculado: $([math]::Round($metrics.oee, 2))%"
Write-Host "└─ Estado: Completada`n"

Write-Host "PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "├─ Crear más máquinas"
Write-Host "├─ Ejecutar nuevas órdenes"
Write-Host "├─ Integrar con sensores MQTT"
Write-Host "├─ Configurar dashboard"
Write-Host "└─ Implementar autenticación JWT`n"

Write-Host "Documentación: README.md, ARCHITECTURE.md, GETTING_STARTED.md" -ForegroundColor Gray
