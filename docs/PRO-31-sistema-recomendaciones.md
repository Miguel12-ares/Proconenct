# PRO-31 Sistema de Recomendaciones de Profesionales

## Descripción General
Este módulo implementa un sistema de recomendaciones básicas para sugerir profesionales relevantes a los clientes, combinando criterios de calificación, popularidad, actividad reciente y proximidad geográfica. El objetivo es ayudar a los usuarios a encontrar profesionales destacados de manera rápida y precisa.

## Lógica del Algoritmo
- **Score de recomendación**: Se calcula para cada profesional combinando:
  - Calificación promedio (peso 0.5)
  - Número de reseñas (peso 0.2, máximo boost hasta 50 reseñas)
  - Actividad reciente (peso 0.2, perfiles actualizados en los últimos 30 días tienen mayor score)
  - Proximidad geográfica (peso 0.1, si la ubicación del usuario coincide con la del profesional)
- **Diversidad**: En el top 10, no se muestran más de 2 profesionales de la misma especialidad.
- **Fallback**: Siempre se retornan al menos 5 recomendaciones, aunque no se cumplan todos los criterios de diversidad.

## Endpoint API
- **Ruta**: `GET /api/recommendations/professionals`
- **Parámetros opcionales**:
  - `userLocation`: (string) Ubicación del usuario para personalizar resultados.
  - `limit`: (int) Número máximo de recomendaciones (por defecto 10).
- **Respuesta**: Lista de objetos `ProfessionalRecommendationDto` con los campos principales del perfil y el score calculado.

## Funcionamiento
1. El controlador `RecommendationsController` expone el endpoint REST.
2. El servicio `RecommendationService` obtiene los perfiles activos y completos desde el repositorio.
3. Se calcula el score para cada profesional y se ordenan los resultados.
4. Se aplica la lógica de diversidad y fallback.
5. Se retorna la lista de recomendaciones al cliente.

## Ubicación del Código
- **DTO**: `ProConnect.Application/DTOs/ProfessionalRecommendationDto.cs`
- **Interfaz**: `ProConnect.Core/Interfaces/IRecommendationService.cs`
- **Servicio**: `ProConnect.Application/Services/RecommendationService.cs`
- **Controlador**: `Controllers/RecommendationsController.cs`
- **Registro DI**: `Program.cs`

## Mantenimiento y Extensión
- Para ajustar los pesos del algoritmo, modificar el método `CalcularScore` en `RecommendationService`.
- Para agregar nuevos criterios (ej. disponibilidad, servicios ofrecidos), extender la lógica en el mismo servicio.
- Para cambiar la diversidad, ajustar la lógica de conteo de especialidades en el método principal.
- El endpoint es fácilmente extensible para recibir más parámetros de personalización.

## Pruebas y Validación
- Probar el endpoint en Swagger (`/swagger`), ruta `GET /api/recommendations/professionals`.
- Verificar que:
  - Se reciben al menos 5 recomendaciones.
  - No hay más de 2 profesionales de la misma especialidad en el top 10.
  - Los resultados son relevantes y el tiempo de respuesta es rápido.

## Consideraciones
- El sistema está optimizado para consultas rápidas (<200ms) y puede ser cacheado si se requiere mayor performance.
- Las recomendaciones se actualizan en tiempo real al consultar el endpoint, pero pueden programarse tareas diarias para cachear resultados si la base de datos crece.

---
**Actualizado:** [Fecha de implementación] 