# PRO-25: Gestión de Portafolio y Credenciales - API

## Resumen
Implementación de los endpoints REST para la gestión de portafolio profesional en ProConnect. Esta funcionalidad permite a los usuarios tipo "Professional" subir, listar, actualizar y eliminar archivos de su portafolio (imágenes, documentos y credenciales), cumpliendo con los criterios de aceptación del sprint 2.

---

## ¿Qué hace esta funcionalidad?
Permite a los profesionales registrados:
- Subir archivos a su portafolio (imágenes jpg/png y documentos pdf)
- Listar todos los archivos de su portafolio
- Actualizar la descripción de un archivo
- Eliminar archivos de su portafolio
- Validaciones de tipo, tamaño y cantidad de archivos

---

## Endpoints REST implementados

| Método | Ruta                                         | Descripción                                   | Autenticación |
|--------|----------------------------------------------|-----------------------------------------------|---------------|
| POST   | `/api/professionals/portfolio/upload`        | Subir archivo al portafolio                   | Professional  |
| GET    | `/api/professionals/portfolio`               | Listar archivos del portafolio propio         | Professional  |
| PUT    | `/api/professionals/portfolio/{id}`          | Actualizar descripción de un archivo          | Professional  |
| DELETE | `/api/professionals/portfolio/{id}`          | Eliminar archivo del portafolio               | Professional  |

---

## Funcionamiento y flujo

1. **Subida de archivo**
   - Solo usuarios autenticados como "Professional" pueden subir archivos.
   - Se valida el tipo de archivo (jpg, png, pdf), tamaño máximo (5MB) y límite de archivos por usuario (10).
   - El archivo se almacena en la carpeta `wwwroot/portfolio/{userId}` y se guarda el metadato en la base de datos MongoDB.

2. **Listado de archivos**
   - El usuario autenticado puede consultar todos los archivos de su portafolio.
   - Se retorna una lista de DTOs con los datos de cada archivo.

3. **Actualización de descripción**
   - Solo el propietario puede actualizar la descripción de un archivo de su portafolio.

4. **Eliminación de archivo**
   - Solo el propietario puede eliminar archivos de su portafolio.
   - Se elimina tanto el archivo físico como el registro en la base de datos.

---

## Ubicación del código

- **Controlador:**
  - `Controllers/PortfolioController.cs`
  - Define las rutas, validaciones de entrada y manejo de errores HTTP.

- **Servicio de aplicación:**
  - `ProConnect.Application/Services/PortfolioService.cs`
  - Contiene la lógica de negocio, validaciones y mapeo de entidades a DTOs.

- **DTOs:**
  - `ProConnect.Application/DTOs/PortfolioFileDto.cs`

- **Repositorio:**
  - `ProConnect.Infrastructure/Repositores/PortfolioRepository.cs`
  - Acceso a la base de datos MongoDB para la colección `portfolioFiles`.

- **Entidades:**
  - `ProConnect.Core/Entities/PortfolioFile.cs`

- **Interfaces:**
  - `ProConnect.Application/Interfaces/IPortfolioService.cs`
  - `ProConnect.Core/Interfaces/IPortfolioRepository.cs`

- **Almacenamiento de archivos:**
  - Carpeta física: `wwwroot/portfolio/{userId}/`

---

## Validaciones y seguridad
- Solo usuarios con rol `Professional` pueden gestionar archivos de portafolio.
- Validación de tipo de archivo, tamaño máximo y cantidad máxima por usuario.
- Control de acceso y autorización mediante JWT y atributos `[Authorize(Roles = "Professional")]`.
- Manejo de errores con mensajes claros y códigos HTTP apropiados.

---

## Mantenimiento y futuras actualizaciones

- **Extensión de campos:**
  - Para agregar nuevos campos a los archivos de portafolio, actualiza la entidad `PortfolioFile`, el DTO correspondiente y el servicio de mapeo.

- **Cambios en reglas de negocio:**
  - Modifica la lógica en `PortfolioService` para nuevas reglas o flujos.

- **Almacenamiento:**
  - Para cambiar la ubicación o el proveedor de almacenamiento, actualiza la lógica de guardado en `PortfolioService`.

- **Documentación:**
  - Mantén este archivo actualizado ante cualquier cambio relevante en los endpoints o lógica de negocio.

---

## Ejemplo de uso (Swagger/Postman)

1. **Autorízate con un usuario tipo Professional**
2. Prueba los endpoints desde `/swagger` o usando Postman:
   - Subir archivo (`POST /api/professionals/portfolio/upload`)
   - Listar archivos (`GET /api/professionals/portfolio`)
   - Actualizar descripción (`PUT /api/professionals/portfolio/{id}`)
   - Eliminar archivo (`DELETE /api/professionals/portfolio/{id}`)

---

## Contacto y soporte
Para dudas o soporte sobre esta funcionalidad, contactar al equipo de backend o revisar la documentación técnica en el repositorio. 