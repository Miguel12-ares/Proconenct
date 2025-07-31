# Script para actualizar la base de datos ProConnect con los nuevos campos de documento
# Este script debe ejecutarse después de implementar los cambios en el código

Write-Host "=== Actualización de Base de Datos ProConnect ===" -ForegroundColor Green
Write-Host "Este script actualizará los usuarios existentes con campos de documento por defecto" -ForegroundColor Yellow

# Configuración de MongoDB
$connectionString = "mongodb://localhost:27017"
$databaseName = "ProConnectDB"
$collectionName = "users"

try {
    # Importar el módulo de MongoDB si está disponible
    if (-not (Get-Module -ListAvailable -Name MongoDB.Driver)) {
        Write-Host "Instalando MongoDB.Driver..." -ForegroundColor Yellow
        Install-Module -Name MongoDB.Driver -Force -Scope CurrentUser
    }
    
    Import-Module MongoDB.Driver
    
    # Conectar a MongoDB
    Write-Host "Conectando a MongoDB..." -ForegroundColor Yellow
    $client = New-Object MongoDB.Driver.MongoClient($connectionString)
    $database = $client.GetDatabase($databaseName)
    $collection = $database.GetCollection("users")
    
    # Buscar usuarios que no tienen los nuevos campos
    Write-Host "Buscando usuarios sin campos de documento..." -ForegroundColor Yellow
    $filter = @{
        '$or' = @(
            @{ 'documentId' = @{ '$exists' = $false } },
            @{ 'documentType' = @{ '$exists' = $false } }
        )
    }
    
    $usersWithoutDocFields = $collection.Find($filter).ToList()
    
    if ($usersWithoutDocFields.Count -eq 0) {
        Write-Host "Todos los usuarios ya tienen los campos de documento configurados." -ForegroundColor Green
        exit 0
    }
    
    Write-Host "Encontrados $($usersWithoutDocFields.Count) usuarios para actualizar." -ForegroundColor Yellow
    
    # Actualizar cada usuario
    $updatedCount = 0
    foreach ($user in $usersWithoutDocFields) {
        $updateFilter = @{ '_id' = $user._id }
        $update = @{
            '$set' = @{
                'documentId' = "0000000000"  # Valor por defecto
                'documentType' = 1  # CC por defecto
            }
        }
        
        try {
            $result = $collection.UpdateOne($updateFilter, $update)
            if ($result.ModifiedCount -gt 0) {
                $updatedCount++
                Write-Host "Usuario $($user.email) actualizado con campos de documento por defecto." -ForegroundColor Green
            }
        }
        catch {
            Write-Host "Error al actualizar usuario $($user.email): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host "=== Resumen de Actualización ===" -ForegroundColor Green
    Write-Host "Usuarios procesados: $($usersWithoutDocFields.Count)" -ForegroundColor White
    Write-Host "Usuarios actualizados: $updatedCount" -ForegroundColor Green
    
    if ($updatedCount -gt 0) {
        Write-Host "`nIMPORTANTE: Los usuarios actualizados tienen valores por defecto para los campos de documento." -ForegroundColor Yellow
        Write-Host "Los usuarios deben actualizar su información en el perfil para proporcionar sus datos reales." -ForegroundColor Yellow
    }
    
    # Crear índices únicos para los nuevos campos
    Write-Host "`nCreando índices únicos para los nuevos campos..." -ForegroundColor Yellow
    
    # Índice único para documentId
    $indexModel = New-Object MongoDB.Driver.CreateIndexModel(
        @{ 'documentId' = 1 },
        @{ 'unique' = $true }
    )
    $collection.Indexes.CreateOne($indexModel)
    Write-Host "Índice único creado para documentId" -ForegroundColor Green
    
    # Índice para email (si no existe)
    $emailIndexModel = New-Object MongoDB.Driver.CreateIndexModel(
        @{ 'email' = 1 },
        @{ 'unique' = $true }
    )
    $collection.Indexes.CreateOne($emailIndexModel)
    Write-Host "Índice único creado para email" -ForegroundColor Green
    
    Write-Host "`n=== Actualización Completada ===" -ForegroundColor Green
    Write-Host "La base de datos ha sido actualizada exitosamente." -ForegroundColor Green
}
catch {
    Write-Host "Error durante la actualización: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Asegúrate de que MongoDB esté ejecutándose y sea accesible." -ForegroundColor Yellow
    exit 1
} 