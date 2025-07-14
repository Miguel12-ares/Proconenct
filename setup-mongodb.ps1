# Script para configurar MongoDB en Windows
# Ejecutar como administrador

Write-Host "=== Configuración de MongoDB para ProConnect ===" -ForegroundColor Green

# Verificar si MongoDB ya está instalado
$mongoPath = Get-Command mongod -ErrorAction SilentlyContinue

if ($mongoPath) {
    Write-Host "MongoDB ya está instalado en: $($mongoPath.Source)" -ForegroundColor Yellow
} else {
    Write-Host "MongoDB no está instalado. Instalando..." -ForegroundColor Yellow
    
    # Instalar MongoDB usando Chocolatey (si está disponible)
    if (Get-Command choco -ErrorAction SilentlyContinue) {
        Write-Host "Instalando MongoDB usando Chocolatey..." -ForegroundColor Cyan
        choco install mongodb -y
    } else {
        Write-Host "Chocolatey no está disponible. Por favor, instala MongoDB manualmente:" -ForegroundColor Red
        Write-Host "1. Descarga MongoDB Community Server desde: https://www.mongodb.com/try/download/community" -ForegroundColor White
        Write-Host "2. Instala MongoDB como servicio" -ForegroundColor White
        Write-Host "3. Ejecuta este script nuevamente" -ForegroundColor White
        exit 1
    }
}

# Crear directorio de datos si no existe
$dataDir = "C:\data\db"
if (!(Test-Path $dataDir)) {
    Write-Host "Creando directorio de datos: $dataDir" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $dataDir -Force
}

# Verificar si MongoDB está ejecutándose
$mongoProcess = Get-Process mongod -ErrorAction SilentlyContinue

if ($mongoProcess) {
    Write-Host "MongoDB ya está ejecutándose (PID: $($mongoProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "Iniciando MongoDB..." -ForegroundColor Cyan
    
    # Intentar iniciar MongoDB como servicio
    try {
        Start-Service MongoDB
        Write-Host "MongoDB iniciado como servicio" -ForegroundColor Green
    } catch {
        Write-Host "No se pudo iniciar MongoDB como servicio. Iniciando manualmente..." -ForegroundColor Yellow
        
        # Iniciar MongoDB manualmente
        Start-Process mongod -ArgumentList "--dbpath", $dataDir -WindowStyle Hidden
        Start-Sleep -Seconds 3
        
        $mongoProcess = Get-Process mongod -ErrorAction SilentlyContinue
        if ($mongoProcess) {
            Write-Host "MongoDB iniciado manualmente (PID: $($mongoProcess.Id))" -ForegroundColor Green
        } else {
            Write-Host "Error: No se pudo iniciar MongoDB" -ForegroundColor Red
            exit 1
        }
    }
}

# Verificar conexión
Write-Host "Verificando conexión a MongoDB..." -ForegroundColor Cyan
try {
    $result = mongo --eval "db.runCommand('ping')" --quiet
    if ($result -like "*ok*") {
        Write-Host "MongoDB está funcionando correctamente!" -ForegroundColor Green
    } else {
        Write-Host "Advertencia: No se pudo verificar la conexión a MongoDB" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Advertencia: No se pudo verificar la conexión a MongoDB" -ForegroundColor Yellow
}

Write-Host "=== Configuración completada ===" -ForegroundColor Green
Write-Host "Ahora puedes ejecutar: dotnet run" -ForegroundColor Cyan 