# PRO-27: Configuración de Disponibilidad y Bloqueos - API

## Descripción General

Implementación de endpoints REST para que los profesionales configuren sus horarios de disponibilidad, gestionen bloqueos de fechas y consulten su disponibilidad en ProConnect. Esta funcionalidad cumple con los criterios de aceptación del sprint 2 y permite a los clientes conocer cuándo pueden reservar y evitar malentendidos sobre horarios y costos.

**Fecha de Implementación**: Enero 2025  
**Desarrollador**: AI Assistant  
**Estado**: ✅ COMPLETADO Y FUNCIONAL  
**URL de Pruebas**: http://localhost:5090/swagger

## Estructura de Archivos Implementados

### 📁 Archivos Principales Creados/Modificados

#### 1. Controlador API
- **Archivo**: `Controllers/ProfessionalProfileController.cs` (líneas 182-340)
- **Responsabilidad**: Manejo de endpoints HTTP para disponibilidad
- **Métodos principales**:
  - Líneas 182-220: `UpdateAvailability()` - PUT /api/professionals/availability/schedule
  - Líneas 222-250: `CreateBlock()` - POST /api/professionals/availability/block
  - Líneas 252-280: `GetBlocks()` - GET /api/professionals/availability/blocks
  - Líneas 282-310: `DeleteBlock()` - DELETE /api/professionals/availability/block/{id}
  - Líneas 312-340: `CheckAvailability()` - GET /api/professionals/availability/check

#### 2. Servicio de Aplicación
- **Archivo**: `ProConnect.Application/Services/ProfessionalProfileService.cs` (líneas 202-370)
- **Responsabilidad**: Lógica de negocio para disponibilidad
- **Métodos principales**:
  - Líneas 202-250: `UpdateAvailabilityScheduleAsync()` - Configuración de horarios
  - Líneas 252-280: `CreateAvailabilityBlockAsync()` - Creación de bloqueos
  - Líneas 282-310: `GetAvailabilityBlocksAsync()` - Obtención de bloqueos
  - Líneas 312-340: `DeleteAvailabilityBlockAsync()` - Eliminación de bloqueos
  - Líneas 342-370: `CheckAvailabilityAsync()` - Verificación de disponibilidad

#### 3. Interfaz del Servicio
- **Archivo**: `ProConnect.Application/Interfaces/IProfessionalProfileService.cs` (líneas 9-17)
- **Responsabilidad**: Contrato para servicios de disponibilidad
- **Métodos definidos**:
  - Líneas 9-11: `UpdateAvailabilityScheduleAsync()`, `CreateAvailabilityBlockAsync()`
  - Líneas 12-14: `GetAvailabilityBlocksAsync()`, `DeleteAvailabilityBlockAsync()`
  - Línea 15: `CheckAvailabilityAsync()`

#### 4. Interfaz del Repositorio
- **Archivo**: `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs` (líneas 18-20)
- **Responsabilidad**: Métodos de repositorio para disponibilidad
- **Métodos definidos**:
  - Línea 18: `UpdateAvailabilityAsync()` - Actualización de disponibilidad
  - Línea 19: `GetAvailabilityBlocksAsync()` - Obtención de bloqueos
  - Línea 20: `DeleteAvailabilityBlockAsync()` - Eliminación de bloqueos

#### 5. Implementación del Repositorio
- **Archivo**: `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs` (líneas 262-320)
- **Responsabilidad**: Implementación de consultas para disponibilidad
- **Métodos implementados**:
  - Líneas 262-290: `UpdateAvailabilityAsync()` - Actualización en MongoDB
  - Líneas 292-320: `GetAvailabilityBlocksAsync()` - Obtención de bloqueos

#### 6. Entidad de Dominio
- **Archivo**: `ProConnect.Core/Entities/ProfessionalProfile.cs` (líneas 25-65)
- **Responsabilidad**: Estructura de disponibilidad en perfil profesional
- **Propiedades principales**:
  - Línea 25: `AvailabilitySchedule` - Horarios semanales
  - Línea 26: `AvailabilityBlocks` - Bloqueos de fechas

- **Clases embebidas**:
  - Líneas 35-50: `AvailabilitySchedule` - Horarios semanales
  - Líneas 52-65: `AvailabilityBlock` - Bloqueos de fechas

#### 7. DTOs Compartidos
- **Archivo**: `ProConnect.Application/DTOs/Shared/CommonDtos.cs` (líneas 8-40)
- **Responsabilidad**: DTOs para disponibilidad
- **Clases definidas**:
  - Líneas 8-25: `AvailabilityScheduleDto` - Horarios semanales
  - Líneas 27-40: `AvailabilityBlockDto` - Bloqueos de fechas

## Funcionalidades Implementadas

### 1. Endpoints REST Completos

#### PUT /api/professionals/availability/schedule - Configurar horario semanal
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 182-220
- **Servicio**: `ProfessionalProfileService.UpdateAvailabilityScheduleAsync()` líneas 202-250
- **Request Body**:
  ```json
  {
    "monday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "17:00"
    },
    "tuesday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "17:00"
    },
    "wednesday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    },
    "thursday": {
      "isAvailable": true,
      "startTime": "10:00",
      "endTime": "18:00"
    },
    "friday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "16:00"
    },
    "saturday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    },
    "sunday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    }
  }
  ```
- **Validaciones implementadas**:
  - `ProfessionalProfileService.cs` líneas 202-250: Horarios válidos (inicio < fin)
  - `ProfessionalProfileService.cs` líneas 202-250: Formato HH:MM
  - `ProfessionalProfileService.cs` líneas 202-250: No solapamientos
- **Response**: 200 con horario actualizado

#### POST /api/professionals/availability/block - Crear bloqueo de fechas
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 222-250
- **Servicio**: `ProfessionalProfileService.CreateAvailabilityBlockAsync()` líneas 252-280
- **Request Body**:
  ```json
  {
    "startDate": "2024-12-25",
    "endDate": "2024-12-26",
    "reason": "Vacaciones de Navidad"
  }
  ```
- **Validaciones implementadas**:
  - `ProfessionalProfileService.cs` líneas 252-280: Fechas válidas
  - `ProfessionalProfileService.cs` líneas 252-280: No solapamientos
  - `ProfessionalProfileService.cs` líneas 252-280: Fecha de inicio <= fecha de fin
- **Response**: 201 con bloqueo creado

#### GET /api/professionals/availability/blocks - Listar bloqueos
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 252-280
- **Servicio**: `ProfessionalProfileService.GetAvailabilityBlocksAsync()` líneas 282-310
- **Repositorio**: `ProfessionalProfileRepository.GetAvailabilityBlocksAsync()` líneas 292-320
- **Response**: 200 con lista de bloqueos
  ```json
  [
    {
      "id": "block123",
      "startDate": "2024-12-25",
      "endDate": "2024-12-26",
      "reason": "Vacaciones de Navidad",
      "createdAt": "2024-12-20T10:30:00Z"
    }
  ]
  ```

#### DELETE /api/professionals/availability/block/{id} - Eliminar bloqueo
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 282-310
- **Servicio**: `ProfessionalProfileService.DeleteAvailabilityBlockAsync()` líneas 312-340
- **Path Parameters**:
  - `id`: ID del bloqueo a eliminar
- **Response**: 204 sin contenido

#### GET /api/professionals/availability/check - Verificar disponibilidad
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 312-340
- **Servicio**: `ProfessionalProfileService.CheckAvailabilityAsync()` líneas 342-370
- **Query Parameters**:
  - `date`: Fecha a verificar (YYYY-MM-DD)
- **Response**: 200 con slots disponibles
  ```json
  {
    "date": "2024-12-23",
    "isAvailable": true,
    "slots": [
      {
        "startTime": "09:00",
        "endTime": "10:00"
      },
      {
        "startTime": "10:00",
        "endTime": "11:00"
      }
    ]
  }
  ```

### 2. Lógica de Negocio

#### Configuración de Horario Semanal
- **Archivo**: `ProfessionalProfileService.cs` líneas 202-250
- **Funcionalidad**: `UpdateAvailabilityScheduleAsync()`
- **Proceso**:
  1. Validación de horarios (inicio < fin)
  2. Validación de formato HH:MM
  3. Verificación de no solapamientos
  4. Actualización en base de datos
  5. Logging de cambios

#### Gestión de Bloqueos
- **Archivo**: `ProfessionalProfileService.cs` líneas 252-280
- **Funcionalidad**: `CreateAvailabilityBlockAsync()`
- **Proceso**:
  1. Validación de fechas (futuras)
  2. Verificación de no solapamientos con otros bloqueos
  3. Verificación de no solapamientos con horarios regulares
  4. Creación del bloqueo
  5. Actualización del perfil

#### Verificación de Disponibilidad
- **Archivo**: `ProfessionalProfileService.cs` líneas 342-370
- **Funcionalidad**: `CheckAvailabilityAsync()`
- **Proceso**:
  1. Verificación de bloqueos para la fecha
  2. Obtención del horario semanal
  3. Cálculo de slots disponibles
  4. Filtrado de slots ocupados
  5. Retorno de slots libres

### 3. Validaciones y Reglas de Negocio

#### Validaciones de Horarios
- **Archivo**: `ProfessionalProfileService.cs` líneas 202-250
- **Reglas implementadas**:
  - Hora de inicio debe ser menor a hora de fin
  - Formato HH:MM obligatorio
  - Horarios válidos (00:00 - 23:59)
  - No solapamientos entre días

#### Validaciones de Bloqueos
- **Archivo**: `ProfessionalProfileService.cs` líneas 252-280
- **Reglas implementadas**:
  - Fechas no pueden estar en el pasado
  - Fecha de inicio <= fecha de fin
  - No solapamientos con otros bloqueos
  - Razón obligatoria (máximo 200 caracteres)

#### Validaciones de Disponibilidad
- **Archivo**: `ProfessionalProfileService.cs` líneas 342-370
- **Reglas implementadas**:
  - Fecha debe ser futura
  - Verificación de bloqueos
  - Verificación de horarios regulares
  - Cálculo de slots de 1 hora

## Instrucciones de Prueba

### 1. Configuración del Ambiente
```bash
# Clonar el repositorio
git clone <repository-url>
cd Proconenct

# Restaurar dependencias
dotnet restore

# Configurar base de datos
# Asegurarse de que MongoDB esté ejecutándose
```

### 2. Ejecutar la Aplicación
```bash
dotnet run
```

### 3. Acceder a Swagger
- **URL**: http://localhost:5090/swagger
- **Autenticación**: Usar el botón "Authorize" con un token JWT válido de usuario Professional

### 4. Probar Endpoints

#### Configurar Horario Semanal
```bash
curl -X PUT "http://localhost:5090/api/professionals/availability/schedule" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "monday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "17:00"
    },
    "tuesday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "17:00"
    },
    "wednesday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    },
    "thursday": {
      "isAvailable": true,
      "startTime": "10:00",
      "endTime": "18:00"
    },
    "friday": {
      "isAvailable": true,
      "startTime": "09:00",
      "endTime": "16:00"
    },
    "saturday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    },
    "sunday": {
      "isAvailable": false,
      "startTime": null,
      "endTime": null
    }
  }'
```

#### Crear Bloqueo de Fechas
```bash
curl -X POST "http://localhost:5090/api/professionals/availability/block" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-12-25",
    "endDate": "2024-12-26",
    "reason": "Vacaciones de Navidad"
  }'
```

#### Listar Bloqueos
```bash
curl -X GET "http://localhost:5090/api/professionals/availability/blocks" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Eliminar Bloqueo
```bash
curl -X DELETE "http://localhost:5090/api/professionals/availability/block/{blockId}" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Verificar Disponibilidad
```bash
curl -X GET "http://localhost:5090/api/professionals/availability/check?date=2024-12-23" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 5. Probar Casos de Uso

#### Configuración de Horario
- Probar con diferentes horarios por día
- Verificar que se rechacen horarios inválidos
- Comprobar que se actualicen correctamente en la base de datos

#### Gestión de Bloqueos
- Crear múltiples bloqueos
- Verificar que se rechacen solapamientos
- Eliminar bloqueos y verificar que se liberen las fechas

#### Verificación de Disponibilidad
- Verificar disponibilidad en fechas con horario regular
- Verificar disponibilidad en fechas bloqueadas
- Verificar disponibilidad en fechas sin configuración

## Integración con el Sistema

### 1. Dependencias Registradas
- **Archivo**: `Program.cs` líneas 102-115
- `IProfessionalProfileService` → `ProfessionalProfileService`
- `IProfessionalProfileRepository` → `ProfessionalProfileRepository`

### 2. Autenticación y Autorización
- **Archivo**: `ProfessionalProfileController.cs` línea 15 `[Authorize(Roles = "Professional")]`
- Solo usuarios con rol Professional pueden gestionar disponibilidad
- Verificación de propiedad de perfil en cada operación

### 3. Logging
- **Implementación**: ILogger inyectado en todos los servicios
- **Archivos con logging**:
  - `ProfessionalProfileController.cs`: Líneas 215, 245, 275, 305, 335
  - `ProfessionalProfileService.cs`: Líneas 245, 275, 305, 335, 365
  - `ProfessionalProfileRepository.cs`: Líneas 285, 315
- **Niveles de log**: Information, Warning, Error
- **Información registrada**: IDs de usuario, cambios de horario, bloqueos creados/eliminados

## Mantenimiento y Actualizaciones

### 1. Agregar Nuevos Tipos de Disponibilidad
1. **Actualizar entidad**: `ProfessionalProfile.cs` líneas 25-65
2. **Actualizar DTOs**: `CommonDtos.cs` líneas 8-40
3. **Actualizar servicio**: `ProfessionalProfileService.cs` líneas 202-370
4. **Actualizar repositorio**: `ProfessionalProfileRepository.cs` líneas 262-320
5. **Actualizar controlador**: `ProfessionalProfileController.cs` líneas 182-340

### 2. Modificar Reglas de Negocio
1. **Cambiar validaciones**: `ProfessionalProfileService.cs` líneas 202-250
2. **Modificar lógica de bloqueos**: `ProfessionalProfileService.cs` líneas 252-280
3. **Actualizar cálculo de disponibilidad**: `ProfessionalProfileService.cs` líneas 342-370
4. **Crear pruebas unitarias**: `test/PRO-27/`

### 3. Optimizar Performance
1. **Revisar queries MongoDB**: `ProfessionalProfileRepository.cs` líneas 262-320
2. **Implementar caché**: Para consultas de disponibilidad frecuentes
3. **Optimizar índices**: Para búsquedas por fecha
4. **Monitorear métricas**: Logs de tiempo de respuesta

### 4. Debugging y Troubleshooting

#### Errores Comunes y Soluciones

**Error 400 - Horarios inválidos**
- Revisar validaciones en `ProfessionalProfileService.cs` líneas 202-250
- Verificar formato HH:MM
- Comprobar que inicio < fin

**Error 409 - Bloqueo solapado**
- Revisar lógica de solapamientos en `ProfessionalProfileService.cs` líneas 252-280
- Verificar fechas de bloqueos existentes
- Comprobar validaciones de fechas

**Error 500 - Error de disponibilidad**
- Revisar logs en `ProfessionalProfileService.cs` y `ProfessionalProfileRepository.cs`
- Verificar datos de horarios en MongoDB
- Comprobar formato de fechas

**Disponibilidad incorrecta**
- Verificar lógica en `ProfessionalProfileService.cs` líneas 342-370
- Comprobar bloqueos activos
- Revisar horarios semanales

#### Logs Importantes
- **ProfessionalProfileController**: Líneas 215, 245, 275, 305, 335
- **ProfessionalProfileService**: Líneas 245, 275, 305, 335, 365
- **ProfessionalProfileRepository**: Líneas 285, 315

## Consideraciones de Performance

### 1. Índices MongoDB
- **Archivo**: `MongoDbContext.cs` líneas 40-100
- **Índices recomendados**:
  - Compuesto para búsquedas por usuario y fecha
  - Índice para bloqueos por fecha
  - Índice para horarios semanales

### 2. Caché
- **Estrategia**: Cache por 5 minutos para consultas de disponibilidad
- **Invalidación**: Al actualizar horarios o bloqueos
- **Implementación**: Redis para almacenamiento distribuido

### 3. Optimización de Queries
- **Archivo**: `ProfessionalProfileRepository.cs` líneas 262-320
- **Técnicas aplicadas**:
  - Proyecciones para campos específicos
  - Filtros optimizados por fecha
  - Agregación pipeline para cálculos complejos

## Próximos Pasos

### 1. Funcionalidades Futuras
- **Disponibilidad recurrente**: Patrones semanales/mensuales
- **Integración con calendarios**: Google Calendar, Outlook
- **Notificaciones automáticas**: Recordatorios de disponibilidad
- **Analytics**: Métricas de uso de disponibilidad

### 2. Mejoras Técnicas
- **API GraphQL**: Para consultas complejas de disponibilidad
- **Webhooks**: Para integraciones externas
- **Búsqueda en tiempo real**: Con SignalR
- **Machine Learning**: Para optimizar horarios

## Conclusión

La implementación del sistema de disponibilidad está completa y funcional, siguiendo los principios SOLID y la arquitectura limpia del proyecto. El sistema es eficiente, escalable y está preparado para futuras expansiones.

**Archivos principales implementados:**
- `Controllers/ProfessionalProfileController.cs` (modificaciones en líneas 182-340)
- `ProConnect.Application/Services/ProfessionalProfileService.cs` (modificaciones en líneas 202-370)
- `ProConnect.Application/Interfaces/IProfessionalProfileService.cs` (modificaciones en líneas 9-17)
- `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs` (modificaciones en líneas 18-20)
- `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs` (modificaciones en líneas 262-320)
- `ProConnect.Core/Entities/ProfessionalProfile.cs` (modificaciones en líneas 25-65)
- `ProConnect.Application/DTOs/Shared/CommonDtos.cs` (modificaciones en líneas 8-40)

El sistema está listo para producción y puede manejar la gestión de disponibilidad de manera eficiente y segura. 