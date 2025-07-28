# PRO-45: APIs y Validaciones de Gestión de Reseñas (Reviews)

## Objetivo
Documentar los endpoints, validaciones de negocio, flujos de prueba y ejemplos para la funcionalidad de gestión de reseñas entre clientes y profesionales en ProConnect.

---

## Endpoints REST

### 1. Crear una reseña
- **POST** `/api/reviews`
- **Auth:** Cliente autenticado (JWT)
- **Body ejemplo:**
```json
{
  "professionalId": "64e2b3f7c8e4a2b1c4d5e6f8",
  "bookingId": "booking123",
  "rating": 5,
  "comment": "¡Excelente servicio!"
}
```
- **Respuesta exitosa:** 201 Created + ReviewDto
- **Errores:**
  - 400: Ya existe una reseña para esta reserva
  - 400: Solo se pueden reseñar reservas completadas
  - 401: Usuario no autenticado

### 2. Obtener reviews de un profesional
- **GET** `/api/reviews/by-professional/{professionalId}`
- **GET** `/api/reviews/by-professional/{professionalId}/paged?page=1&pageSize=10`

### 3. Obtener reviews de un cliente
- **GET** `/api/reviews/by-client/{clientId}`
- **GET** `/api/reviews/by-client/{clientId}/paged?page=1&pageSize=10`

### 4. Obtener review por ID
- **GET** `/api/reviews/{id}`

### 5. Actualizar o eliminar review
- **PUT** `/api/reviews/{id}`
- **DELETE** `/api/reviews/{id}`

---

## Validaciones de negocio
- Solo clientes autenticados pueden crear reseñas.
- Solo una reseña por combinación cliente/profesional/reserva.
- Solo se puede reseñar una reserva con estado `Completed`.
- El rating debe ser un entero entre 1 y 5.
- El comentario es obligatorio y máximo 500 caracteres.
- El `clientId` se extrae del JWT, no del body.

---

## Ejemplos de prueba (Postman/cURL)

### Crear reseña válida
```bash
curl -X POST https://localhost:5001/api/reviews \
  -H "Authorization: Bearer {TOKEN_CLIENTE}" \
  -H "Content-Type: application/json" \
  -d '{
    "professionalId": "64e2b3f7c8e4a2b1c4d5e6f8",
    "bookingId": "booking123",
    "rating": 5,
    "comment": "¡Excelente servicio!"
  }'
```

### Crear reseña duplicada (debe fallar)
```bash
# Mismo comando, debe retornar 400 con mensaje de error de unicidad
```

### Crear reseña para reserva no completada (debe fallar)
```bash
# Cambia bookingId por uno con status != Completed. Debe retornar 400.
```

### Obtener reviews paginadas
```bash
curl https://localhost:5001/api/reviews/by-professional/64e2b3f7c8e4a2b1c4d5e6f8/paged?page=1&pageSize=10
```

---

## Flujo de prueba recomendado
1. Crea una reserva con estado `Completed` para un cliente y profesional.
2. Autentica como cliente y crea una reseña para esa reserva.
3. Intenta crear una segunda reseña para la misma reserva (debe fallar).
4. Intenta crear una reseña para una reserva no completada (debe fallar).
5. Consulta las reviews por profesional y cliente, en modo paginado y sin paginar.
6. Verifica la estructura de respuesta y los mensajes de error.

---

## Notas técnicas
- El sistema utiliza MongoDB para persistencia.
- El Id de la review se genera automáticamente.
- Todas las validaciones de negocio están centralizadas en el Application Layer.
- El controller extrae el clientId del JWT para máxima seguridad.

---

## Estado actual
- [x] Endpoints implementados y autenticados
- [x] Validaciones de negocio robustas
- [x] Paginación funcional
- [x] Documentación y ejemplos de prueba

---

**Responsable:** Equipo ProConnect
**Fecha:** 2025-07-27
