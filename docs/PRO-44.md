# PRO-44: Implementación de Reviews y Ratings en MongoDB

## Resumen
Se implementó la funcionalidad para que los clientes puedan dejar calificaciones y reseñas sobre los profesionales tras una consulta, cumpliendo con la arquitectura limpia y principios SOLID.

---

## Estructura y Cambios Realizados

### 1. **Dominio y Modelos**
- **Entidad Review**: Definida en `ProConnect.Core/Entities/Review.cs` con los campos requeridos (reviewId, clientId, professionalId, bookingId, rating, comment, createdAt, updatedAt).
- **DTOs**: En `ProConnect.Application/DTOs/ReviewDto.cs` para creación, actualización y consulta de reviews.

### 2. **Capa de Aplicación**
- **Interfaces**: `IReviewService` y `IReviewRepository` en `ProConnect.Application/Interfaces`.
- **Servicios**: `ReviewService` implementa la lógica de negocio y depende de la interfaz `IReviewRepository`.
- **Validadores**: Uso de FluentValidation para asegurar rating (1-5), comentario opcional (máx 500 caracteres), y presencia de IDs requeridos.

### 3. **Infraestructura**
- **Repositorio**: `ReviewRepository` implementa `IReviewRepository` usando MongoDB Driver.
- **MongoDbContext**: Se agregó la colección `Reviews` y la creación de índices para `professionalId`, `clientId` y `createdAt`.
- **Inyección de Dependencias**: Todo registrado correctamente en `Program.cs`.

### 4. **API**
- **Controller**: `ReviewsController` en `/Controllers` expone endpoints REST para crear, consultar, actualizar y eliminar reviews.

---

## Endpoints Disponibles

- `GET    /api/reviews/{id}`                → Obtener review por ID
- `GET    /api/reviews/by-professional/{id}`→ Listar reviews de un profesional
- `GET    /api/reviews/by-client/{id}`      → Listar reviews de un cliente
- `POST   /api/reviews`                     → Crear review
- `PUT    /api/reviews/{id}`                → Actualizar review
- `DELETE /api/reviews/{id}`                → Eliminar review

---

## Validaciones y Reglas de Negocio
- **Rating**: Debe estar entre 1 y 5.
- **Comentario**: Opcional, máximo 500 caracteres.
- **Referencias**: Deben existir clientId, professionalId y bookingId.
- **Integridad**: Se recomienda validar que el booking pertenezca al cliente y profesional antes de permitir la review (puede implementarse como mejora).

---

## Pruebas y Verificación

### 1. **Swagger**
- Levanta la solución (`dotnet run` o desde tu IDE).
- Accede a `https://localhost:{puerto}/swagger`.
- Busca la sección `Reviews` y prueba los endpoints:
  - **Crear**: Usa el endpoint `POST /api/reviews` con un JSON como:
    ```json
    {
      "clientId": "...",
      "professionalId": "...",
      "bookingId": "...",
      "rating": 5,
      "comment": "Excelente servicio!"
    }
    ```
  - **Consultar**: Prueba los endpoints de consulta por ID, profesional o cliente.
  - **Actualizar/Eliminar**: Usa los endpoints `PUT` y `DELETE` para modificar o borrar reviews.

### 2. **Validación de Índices**
- Verifica en MongoDB Compass que la colección `reviews` tiene los índices esperados (`professionalId`, `clientId`, `createdAt`).

### 3. **Validación Manual**
- Intenta crear reviews con ratings fuera de rango o comentarios muy largos para verificar que la validación funciona.

---

## Notas Finales
- El código sigue la arquitectura limpia y es fácilmente extensible.
- Puedes agregar lógica adicional para evitar reviews duplicadas o validar que solo clientes con booking puedan dejar review.

---

## Contacto
Para dudas o mejoras, consulta la documentación interna o contacta al equipo de backend.
