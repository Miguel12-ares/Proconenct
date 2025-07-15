# PRO-24: Gestión de Perfil Profesional - API

## Resumen
Implementación de los endpoints REST para la creación, consulta, actualización y visualización pública de perfiles profesionales en ProConnect. Esta funcionalidad permite a los usuarios tipo "Professional" gestionar su perfil detallado, cumpliendo con los criterios de aceptación del sprint 2.

---

## ¿Qué hace esta funcionalidad?
Permite a los profesionales registrados:
- Crear su perfil profesional desde cero
- Editar y actualizar su perfil
- Consultar su propio perfil (vista privada)
- Consultar perfiles públicos de otros profesionales (vista pública, sin datos sensibles)
- Validaciones estrictas de datos y control de acceso por roles

---

## Endpoints REST implementados

| Método | Ruta                                 | Descripción                                 | Autenticación |
|--------|--------------------------------------|------------------------------------------------|---------------|
| POST   | `/api/professionals/profile`         | Crear perfil profesional                     | Professional  |
| GET    | `/api/professionals/profile`         | Obtener perfil propio (privado)              | Professional  |
| GET    | `/api/professionals/profile/{id}`    | Obtener perfil público por ID                | No            |
| PUT    | `/api/professionals/profile`         | Actualizar perfil profesional                | Professional  |

---

## Funcionamiento y flujo

1. **Creación de perfil**
   - Solo usuarios autenticados como "Professional" pueden crear su perfil.
   - Se valida que no exista un perfil previo para el usuario.
   - Se validan campos obligatorios: especialidades, bio (mín. 100 caracteres), tarifa, ubicación, etc.
   - El perfil se almacena en la colección `professionalProfiles` de MongoDB.

2. **Consulta de perfil propio**
   - El usuario autenticado puede consultar su perfil completo, incluyendo datos personales.
   - Se retorna un DTO con todos los campos relevantes y datos de usuario.

3. **Consulta de perfil público**
   - Cualquier usuario (autenticado o no) puede consultar un perfil profesional por ID.
   - Solo se muestran datos públicos: nombre completo, especialidades, bio, experiencia, ubicación, etc. (sin email ni datos sensibles).

4. **Actualización de perfil**
   - Solo el propietario del perfil puede actualizarlo.
   - Se validan nuevamente todos los campos obligatorios y reglas de negocio.
   - Se actualiza la fecha de modificación.

---

## Ubicación del código

- **Controlador:**
  - `Controllers/ProfessionalProfileController.cs`
  - Define las rutas, validaciones de entrada y manejo de errores HTTP.

- **Servicio de aplicación:**
  - `ProConnect.Application/Services/ProfessionalProfileService.cs`
  - Contiene la lógica de negocio, validaciones adicionales y mapeo de entidades a DTOs.

- **DTOs:**
  - `ProConnect.Application/DTOs/CreateProfessionalProfileDto.cs`
  - `ProConnect.Application/DTOs/UpdateProfessionalProfileDto.cs`
  - `ProConnect.Application/DTOs/ProfessionalProfileResponseDto.cs`
  - `ProConnect.Application/DTOs/Shared/CommonDtos.cs`

- **Repositorio:**
  - `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs`
  - Acceso a la base de datos MongoDB para la colección `professionalProfiles`.

- **Entidades:**
  - `ProConnect.Core/Entities/ProfessionalProfile.cs`
  - `ProConnect.Core/Entities/User.cs`

- **Validadores:**
  - `ProConnect.Application/Validators/CreateProfessionalProfileValidator.cs`
  - `ProConnect.Application/Validators/UpdateProfessionalProfileValidator.cs`

---

## Validaciones y seguridad
- Solo usuarios con rol `Professional` pueden crear/editar perfiles.
- Validación de datos obligatorios y reglas de negocio en DTOs y servicios.
- Control de acceso y autorización mediante JWT y atributos `[Authorize(Roles = "Professional")]`.
- Manejo de errores con mensajes claros y códigos HTTP apropiados.
- Filtrado de información sensible en la vista pública.

---

## Mantenimiento y futuras actualizaciones

- **Extensión de campos:**
  - Para agregar nuevos campos al perfil profesional, actualiza la entidad `ProfessionalProfile`, los DTOs correspondientes y el servicio de mapeo.
  - Añade validaciones en los validadores FluentValidation.

- **Cambios en reglas de negocio:**
  - Modifica la lógica en `ProfessionalProfileService` para nuevas reglas o flujos.

- **Seguridad:**
  - Si se agregan nuevos roles o permisos, actualiza los atributos `[Authorize]` en el controlador.

- **Documentación:**
  - Mantén este archivo actualizado ante cualquier cambio relevante en los endpoints o lógica de negocio.

- **Pruebas:**
  - Se recomienda agregar pruebas unitarias y de integración para los servicios y controladores.

---

## Ejemplo de uso (Swagger)

1. **Autorízate con un usuario tipo Professional**
2. Prueba los endpoints desde `/swagger`:
   - Crear perfil
   - Consultar perfil propio
   - Consultar perfil público
   - Actualizar perfil

---

## Contacto y soporte
Para dudas o soporte sobre esta funcionalidad, contactar al equipo de backend o revisar la documentación técnica en el repositorio. 