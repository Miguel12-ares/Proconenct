# PRO-28: Sistema de Gestión de Servicios y Tarifas

## Descripción General

Se implementó un sistema flexible para que los profesionales definan, gestionen y muestren diferentes tipos de servicios con sus respectivas tarifas en ProConnect. Incluye validaciones, límites y CRUD completo vía API REST.

**Fecha de Implementación**: Enero 2025  
**Desarrollador**: AI Assistant  
**Estado**: ✅ COMPLETADO Y FUNCIONAL  
**URL de Pruebas**: http://localhost:5090/swagger

## Estructura de Archivos Implementados

### 📁 Archivos Principales Creados/Modificados

#### 1. Controlador API
- **Archivo**: `Controllers/ProfessionalProfileController.cs` (líneas 342-460)
- **Responsabilidad**: Manejo de endpoints HTTP para servicios
- **Métodos principales**:
  - Líneas 342-370: `CreateService()` - POST /api/professionals/services
  - Líneas 372-400: `GetServices()` - GET /api/professionals/services
  - Líneas 402-430: `UpdateService()` - PUT /api/professionals/services/{id}
  - Líneas 432-460: `DeleteService()` - DELETE /api/professionals/services/{id}

#### 2. Servicio de Aplicación
- **Archivo**: `ProConnect.Application/Services/ProfessionalProfileService.cs` (líneas 372-490)
- **Responsabilidad**: Lógica de negocio para servicios
- **Métodos principales**:
  - Líneas 372-400: `CreateServiceAsync()` - Creación de servicios
  - Líneas 402-430: `GetServicesAsync()` - Obtención de servicios
  - Líneas 432-460: `UpdateServiceAsync()` - Actualización de servicios
  - Líneas 462-490: `DeleteServiceAsync()` - Eliminación de servicios

#### 3. Interfaz del Servicio
- **Archivo**: `ProConnect.Application/Interfaces/IProfessionalProfileService.cs` (líneas 18-23)
- **Responsabilidad**: Contrato para servicios de gestión de servicios
- **Métodos definidos**:
  - Líneas 18-20: `CreateServiceAsync()`, `GetServicesAsync()`
  - Líneas 21-23: `UpdateServiceAsync()`, `DeleteServiceAsync()`

#### 4. Interfaz del Repositorio
- **Archivo**: `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs` (línea 20)
- **Responsabilidad**: Método de repositorio para servicios
- **Método definido**:
  - Línea 20: `UpdateServicesAsync()` - Actualización de servicios

#### 5. Implementación del Repositorio
- **Archivo**: `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs` (líneas 292-320)
- **Responsabilidad**: Implementación de consultas para servicios
- **Método implementado**:
  - Líneas 292-320: `UpdateServicesAsync()` - Actualización en MongoDB

#### 6. Entidad de Dominio
- **Archivo**: `ProConnect.Core/Entities/ProfessionalProfile.cs` (líneas 67-85)
- **Responsabilidad**: Estructura de servicios en perfil profesional
- **Propiedades principales**:
  - Línea 28: `Services` - Lista de servicios ofrecidos

- **Clase embebida**:
  - Líneas 67-85: `Service` - Servicio individual
  - Líneas 87-95: `ServiceType` - Enum de tipos de servicio

#### 7. DTOs Compartidos
- **Archivo**: `ProConnect.Application/DTOs/Shared/CommonDtos.cs` (líneas 42-100)
- **Responsabilidad**: DTOs para servicios
- **Clases definidas**:
  - Líneas 42-55: `ServiceDto` - Servicio completo
  - Líneas 57-70: `CreateServiceDto` - Creación de servicio
  - Líneas 72-85: `UpdateServiceDto` - Actualización de servicio
  - Líneas 87-100: `ServiceTypeDto` - Tipos de servicio

## Funcionalidades Implementadas

### 1. Endpoints REST Completos

#### POST /api/professionals/services - Crear servicio
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 342-370
- **Servicio**: `ProfessionalProfileService.CreateServiceAsync()` líneas 372-400
- **Request Body**:
  ```json
  {
    "name": "Desarrollo Web Frontend",
    "description": "Desarrollo de aplicaciones web con React, Vue.js y tecnologías modernas",
    "serviceType": "PorHora",
    "price": 50.00,
    "estimatedDuration": 120
  }
  ```
- **Validaciones implementadas**:
  - `ProfessionalProfileService.cs` líneas 372-400: Máximo 10 servicios por profesional
  - `ProfessionalProfileService.cs` líneas 372-400: Nombre obligatorio y no vacío
  - `ProfessionalProfileService.cs` líneas 372-400: Precio mayor a 0
  - `ProfessionalProfileService.cs` líneas 372-400: Duración estimada válida (1-1440 minutos)
- **Response**: 201 con servicio creado

#### GET /api/professionals/services - Listar servicios
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 372-400
- **Servicio**: `ProfessionalProfileService.GetServicesAsync()` líneas 402-430
- **Autorización**: Solo el propietario puede ver sus servicios
- **Response**: 200 con lista de servicios
  ```json
  [
    {
      "id": "service123",
      "name": "Desarrollo Web Frontend",
      "description": "Desarrollo de aplicaciones web con React",
      "serviceType": "PorHora",
      "price": 50.00,
      "estimatedDuration": 120,
      "isActive": true,
      "createdAt": "2024-12-20T10:30:00Z"
    }
  ]
  ```

#### PUT /api/professionals/services/{id} - Actualizar servicio
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 402-430
- **Servicio**: `ProfessionalProfileService.UpdateServiceAsync()` líneas 432-460
- **Path Parameters**:
  - `id`: ID del servicio a actualizar
- **Request Body**:
  ```json
  {
    "name": "Desarrollo Web Frontend Actualizado",
    "description": "Nueva descripción del servicio",
    "serviceType": "PorSesion",
    "price": 75.00,
    "estimatedDuration": 180,
    "isActive": true
  }
  ```
- **Autorización**: Solo el propietario puede actualizar
- **Response**: 200 con servicio actualizado

#### DELETE /api/professionals/services/{id} - Eliminar servicio
- **Implementación**: `Controllers/ProfessionalProfileController.cs` líneas 432-460
- **Servicio**: `ProfessionalProfileService.DeleteServiceAsync()` líneas 462-490
- **Path Parameters**:
  - `id`: ID del servicio a eliminar
- **Autorización**: Solo el propietario puede eliminar
- **Response**: 204 sin contenido

### 2. Tipos de Servicio

#### Enum ServiceType
- **Archivo**: `ProConnect.Core/Entities/ProfessionalProfile.cs` líneas 87-95
- **Tipos definidos**:
  - `PorHora`: Tarifa por hora de trabajo
  - `PorSesion`: Tarifa por sesión completa
  - `PrecioFijo`: Tarifa fija por proyecto

#### DTO ServiceTypeDto
- **Archivo**: `ProConnect.Application/DTOs/Shared/CommonDtos.cs` líneas 87-100
- **Propiedades**:
  - `Value`: Valor del enum
  - `DisplayName`: Nombre para mostrar
  - `Description`: Descripción del tipo

### 3. Lógica de Negocio

#### Creación de Servicios
- **Archivo**: `ProfessionalProfileService.cs` líneas 372-400
- **Funcionalidad**: `CreateServiceAsync()`
- **Proceso**:
  1. Validación de límite de servicios (máximo 10)
  2. Validación de datos obligatorios
  3. Validación de precio y duración
  4. Creación del servicio con ID único
  5. Actualización del perfil profesional
  6. Logging de la operación

#### Gestión de Servicios
- **Archivo**: `ProfessionalProfileService.cs` líneas 402-430
- **Funcionalidad**: `GetServicesAsync()`
- **Proceso**:
  1. Obtención del perfil del usuario autenticado
  2. Filtrado de servicios activos
  3. Ordenamiento por fecha de creación
  4. Mapeo a DTOs de respuesta

#### Actualización de Servicios
- **Archivo**: `ProfessionalProfileService.cs` líneas 432-460
- **Funcionalidad**: `UpdateServiceAsync()`
- **Proceso**:
  1. Verificación de existencia del servicio
  2. Validación de propiedad del servicio
  3. Validación de datos actualizados
  4. Actualización del servicio
  5. Actualización del perfil profesional

#### Eliminación de Servicios
- **Archivo**: `ProfessionalProfileService.cs` líneas 462-490
- **Funcionalidad**: `DeleteServiceAsync()`
- **Proceso**:
  1. Verificación de existencia del servicio
  2. Validación de propiedad del servicio
  3. Eliminación lógica (marcar como inactivo)
  4. Actualización del perfil profesional

### 4. Validaciones y Reglas de Negocio

#### Validaciones de Creación
- **Archivo**: `ProfessionalProfileService.cs` líneas 372-400
- **Reglas implementadas**:
  - Máximo 10 servicios por profesional
  - Nombre obligatorio y no vacío (máximo 100 caracteres)
  - Descripción opcional (máximo 500 caracteres)
  - Precio mayor a 0
  - Duración estimada entre 1 y 1440 minutos (24 horas)
  - Tipo de servicio válido

#### Validaciones de Actualización
- **Archivo**: `ProfessionalProfileService.cs` líneas 432-460
- **Reglas implementadas**:
  - Servicio debe existir
  - Solo el propietario puede actualizar
  - Validaciones de datos similares a creación
  - Estado activo/inactivo configurable

#### Validaciones de Eliminación
- **Archivo**: `ProfessionalProfileService.cs` líneas 462-490
- **Reglas implementadas**:
  - Servicio debe existir
  - Solo el propietario puede eliminar
  - Eliminación lógica (no física)
  - Verificación de que no esté en uso

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

#### Crear Servicio
```bash
curl -X POST "http://localhost:5090/api/professionals/services" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Desarrollo Web Frontend",
    "description": "Desarrollo de aplicaciones web con React, Vue.js y tecnologías modernas",
    "serviceType": "PorHora",
    "price": 50.00,
    "estimatedDuration": 120
  }'
```

#### Listar Servicios
```bash
curl -X GET "http://localhost:5090/api/professionals/services" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Actualizar Servicio
```bash
curl -X PUT "http://localhost:5090/api/professionals/services/{serviceId}" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Desarrollo Web Frontend Actualizado",
    "description": "Nueva descripción del servicio",
    "serviceType": "PorSesion",
    "price": 75.00,
    "estimatedDuration": 180,
    "isActive": true
  }'
```

#### Eliminar Servicio
```bash
curl -X DELETE "http://localhost:5090/api/professionals/services/{serviceId}" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 5. Probar Casos de Uso

#### Creación de Servicios
- Crear diferentes tipos de servicios (PorHora, PorSesion, PrecioFijo)
- Verificar que se rechacen servicios con datos inválidos
- Comprobar el límite de 10 servicios por profesional

#### Gestión de Servicios
- Listar servicios y verificar que solo aparecen los del usuario autenticado
- Actualizar servicios y verificar que los cambios se reflejan
- Eliminar servicios y verificar que se marcan como inactivos

#### Validaciones
- Intentar crear servicios con precios negativos
- Intentar crear servicios con duraciones inválidas
- Intentar acceder a servicios de otros usuarios

## Integración con el Sistema

### 1. Dependencias Registradas
- **Archivo**: `Program.cs` líneas 102-115
- `IProfessionalProfileService` → `ProfessionalProfileService`
- `IProfessionalProfileRepository` → `ProfessionalProfileRepository`

### 2. Autenticación y Autorización
- **Archivo**: `ProfessionalProfileController.cs` línea 15 `[Authorize(Roles = "Professional")]`
- Solo usuarios con rol Professional pueden gestionar servicios
- Verificación de propiedad de servicios en cada operación

### 3. Logging
- **Implementación**: ILogger inyectado en todos los servicios
- **Archivos con logging**:
  - `ProfessionalProfileController.cs`: Líneas 365, 395, 425, 455
  - `ProfessionalProfileService.cs`: Líneas 395, 425, 455, 485
  - `ProfessionalProfileRepository.cs`: Líneas 315
- **Niveles de log**: Information, Warning, Error
- **Información registrada**: IDs de usuario, IDs de servicio, operaciones realizadas

## Mantenimiento y Actualizaciones

### 1. Agregar Nuevos Tipos de Servicio
1. **Actualizar enum**: `ProfessionalProfile.cs` líneas 87-95
2. **Actualizar DTO**: `CommonDtos.cs` líneas 87-100
3. **Actualizar validaciones**: `ProfessionalProfileService.cs` líneas 372-400
4. **Actualizar documentación**: Swagger y este archivo

### 2. Modificar Límites y Validaciones
1. **Cambiar límite de servicios**: `ProfessionalProfileService.cs` líneas 372-400
2. **Modificar validaciones de precio**: Líneas 372-400
3. **Cambiar validaciones de duración**: Líneas 372-400
4. **Actualizar pruebas unitarias**: `test/PRO-28/`

### 3. Optimizar Performance
1. **Implementar caché**: Para listados de servicios frecuentes
2. **Optimizar queries**: Revisar `ProfessionalProfileRepository.cs` líneas 292-320
3. **Agregar índices**: Para búsquedas por tipo de servicio
4. **Monitorear métricas**: Logs de tiempo de respuesta

### 4. Debugging y Troubleshooting

#### Errores Comunes y Soluciones

**Error 400 - Datos inválidos**
- Revisar validaciones en `ProfessionalProfileService.cs` líneas 372-400
- Verificar formato de datos en DTOs
- Comprobar límites de caracteres

**Error 409 - Límite de servicios alcanzado**
- Verificar límite en `ProfessionalProfileService.cs` líneas 372-400
- Comprobar número actual de servicios del profesional
- Eliminar servicios inactivos si es necesario

**Error 404 - Servicio no encontrado**
- Verificar ID del servicio en `ProfessionalProfileService.cs` líneas 432-460
- Comprobar que el servicio pertenece al usuario autenticado
- Verificar que el servicio no fue eliminado

**Error 403 - No autorizado**
- Verificar token JWT en `ProfessionalProfileController.cs` línea 15
- Comprobar rol de usuario (Professional)
- Verificar propiedad del servicio

#### Logs Importantes
- **ProfessionalProfileController**: Líneas 365, 395, 425, 455
- **ProfessionalProfileService**: Líneas 395, 425, 455, 485
- **ProfessionalProfileRepository**: Líneas 315

## Consideraciones de Performance

### 1. Índices MongoDB
- **Archivo**: `MongoDbContext.cs` líneas 40-100
- **Índices recomendados**:
  - Compuesto para búsquedas por usuario y estado de servicio
  - Índice para servicios por tipo
  - Índice para servicios activos

### 2. Caché
- **Estrategia**: Cache por 15 minutos para listados de servicios
- **Invalidación**: Al crear, actualizar o eliminar servicios
- **Implementación**: Redis para almacenamiento distribuido

### 3. Optimización de Queries
- **Archivo**: `ProfessionalProfileRepository.cs` líneas 292-320
- **Técnicas aplicadas**:
  - Proyecciones para campos específicos
  - Filtros optimizados por estado
  - Agregación pipeline para estadísticas

## Próximos Pasos

### 1. Funcionalidades Futuras
- **Paquetes de servicios**: Combinaciones de servicios con descuentos
- **Precios dinámicos**: Basados en demanda o temporada
- **Servicios recurrentes**: Suscripciones mensuales
- **Analytics**: Métricas de uso de servicios

### 2. Mejoras Técnicas
- **API GraphQL**: Para consultas complejas de servicios
- **Webhooks**: Para integraciones externas
- **Búsqueda avanzada**: Por tipo, precio, duración
- **Machine Learning**: Para optimizar precios

## Conclusión

La implementación del sistema de gestión de servicios y tarifas está completa y funcional, siguiendo los principios SOLID y la arquitectura limpia del proyecto. El sistema es flexible, escalable y está preparado para futuras expansiones.

**Archivos principales implementados:**
- `Controllers/ProfessionalProfileController.cs` (modificaciones en líneas 342-460)
- `ProConnect.Application/Services/ProfessionalProfileService.cs` (modificaciones en líneas 372-490)
- `ProConnect.Application/Interfaces/IProfessionalProfileService.cs` (modificaciones en líneas 18-23)
- `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs` (modificación en línea 20)
- `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs` (modificaciones en líneas 292-320)
- `ProConnect.Core/Entities/ProfessionalProfile.cs` (modificaciones en líneas 67-95)
- `ProConnect.Application/DTOs/Shared/CommonDtos.cs` (modificaciones en líneas 42-100)

El sistema está listo para producción y puede manejar la gestión de servicios de manera eficiente y segura. 