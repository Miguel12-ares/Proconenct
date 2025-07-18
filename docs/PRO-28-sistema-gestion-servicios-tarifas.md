# PRO-28: Sistema de Gestión de Servicios y Tarifas

## Resumen
Se implementó un sistema flexible para que los profesionales definan, gestionen y muestren diferentes tipos de servicios con sus respectivas tarifas en ProConnect. Incluye validaciones, límites y CRUD completo vía API REST.

---

## ¿Qué hace esta funcionalidad?
- Permite a los profesionales:
  - Crear, listar, actualizar y eliminar servicios ofrecidos (máximo 10 por perfil).
  - Definir nombre, descripción, tipo de tarifa (por hora, por sesión, precio fijo), precio y duración estimada.
  - Activar/desactivar servicios.
- Los servicios se almacenan embebidos en el documento del perfil profesional en MongoDB.
- Los servicios se muestran en el perfil público del profesional.

---

## Endpoints REST implementados

| Método | Ruta                                 | Descripción                        | Autenticación  |
|--------|--------------------------------------|------------------------------------|---------------|
| POST   | `/api/professionals/services`        | Crear un nuevo servicio            | Professional  |
| GET    | `/api/professionals/services`        | Listar todos los servicios         | Professional  |
| PUT    | `/api/professionals/services/{id}`   | Actualizar un servicio existente   | Professional  |
| DELETE | `/api/professionals/services/{id}`   | Eliminar un servicio               | Professional  |

---

## Ubicación del código
- **Entidad y modelo:**
  - `ProConnect.Core/Entities/ProfessionalProfile.cs` (clase `Service` y enum `ServiceType`)
- **DTOs:**
  - `ProConnect.Application/DTOs/Shared/CommonDtos.cs` (ServiceDto, CreateServiceDto, UpdateServiceDto, ServiceTypeDto)
- **Repositorio:**
  - `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs`
  - `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs`
- **Servicio de aplicación:**
  - `ProConnect.Application/Services/ProfessionalProfileService.cs`
  - `ProConnect.Application/Interfaces/IProfessionalProfileService.cs`
- **Controlador:**
  - `Controllers/ProfessionalProfileController.cs`

---

## Validaciones y reglas de negocio
- Máximo 10 servicios por profesional.
- Nombre obligatorio y no vacío.
- Precio > 0.
- Duración estimada > 0 y <= 1440 minutos (24h).
- Solo el profesional dueño puede gestionar sus servicios.
- Estado activo/inactivo para cada servicio.

---

## Pruebas y verificación
1. Autentícate como profesional en Swagger.
2. Usa los endpoints `/api/professionals/services` para crear, listar, actualizar y eliminar servicios.
3. Intenta crear más de 10 servicios y verifica que recibes un error.
4. Intenta crear servicios con datos inválidos y verifica los mensajes de error.
5. Consulta el perfil público y verifica que los servicios aparecen correctamente.

---

## Mantenimiento y futuras actualizaciones
- Para agregar nuevos campos o reglas, actualiza la clase `Service`, los DTOs y el servicio.
- Si cambian los tipos de tarifa, actualiza el enum `ServiceType` y su DTO.
- Mantén este archivo actualizado ante cualquier cambio relevante.
- Se recomienda agregar pruebas unitarias para los métodos de servicio y repositorio.

---

## Contacto y soporte
Para dudas o soporte sobre esta funcionalidad, contactar al equipo de backend o revisar la documentación técnica en el repositorio. 