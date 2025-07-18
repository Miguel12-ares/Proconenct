# PRO-28: Busqueda Avanzada de Profesionales

## Descripción
Se implementó un motor de búsqueda avanzada para profesionales en ProConnect, permitiendo a los clientes encontrar perfiles relevantes usando múltiples filtros, ordenamiento y paginación eficiente. La solución sigue principios SOLID y una arquitectura limpia, escalable y mantenible.

## Estructura y Archivos Clave
- **DTOs de Filtros y Resultados:**
  - `ProConnect.Application/DTOs/ProfessionalSearchFiltersDto.cs`
  - `ProConnect.Application/DTOs/ProfessionalSearchResultDto.cs`
  - `ProConnect.Application/DTOs/Shared/PagedResultDto.cs`
- **Modelos de Dominio:**
  - `ProConnect.Core/Models/ProfessionalSearchFilters.cs`
  - `ProConnect.Core/Models/PagedResult.cs`
- **Repositorio:**
  - `ProConnect.Core/Interfaces/IProfessionalProfileRepository.cs` (método `SearchAdvancedAsync`)
  - `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs` (implementación de búsqueda avanzada)
- **Servicio de Búsqueda:**
  - `ProConnect.Application/Interfaces/IProfessionalSearchService.cs`
  - `ProConnect.Application/Services/ProfessionalSearchService.cs`
- **Controlador:**
  - `Controllers/SearchController.cs` (endpoint REST)
- **Registro de dependencias:**
  - `Program.cs`

## Endpoint REST
- **GET /api/search/professionals**
  - Filtros por query string: `query`, `specialties`, `location`, `minHourlyRate`, `maxHourlyRate`, `minRating`, `minExperienceYears`, `virtualConsultation`, `orderBy`, `page`, `pageSize`.
  - Respuesta: resultados paginados, total de coincidencias, datos clave de cada profesional.

## Ejemplo de Uso
```
GET /api/search/professionals?specialties=abogado&location=Medellin&minRating=4&maxHourlyRate=100&page=1&pageSize=20
```

## Mantenimiento y Extensión
- Para agregar nuevos filtros, extiende los DTOs y el modelo `ProfessionalSearchFilters`.
- Para cambiar la lógica de ordenamiento, ajusta el switch en el repositorio.
- El servicio es desacoplado y puede ser probado unitariamente.
- El endpoint está documentado en Swagger y es fácilmente testeable.

## Pruebas
- Accede a `/swagger` y prueba el endpoint con diferentes combinaciones de filtros.
- Verifica que la respuesta sea rápida (<500ms con datos de prueba) y que la paginación funcione correctamente.

## Notas
- El motor está optimizado para MongoDB y puede escalar con índices adecuados.
- El código sigue principios SOLID y clean code para facilitar futuras actualizaciones. 