# PRO-46: Interfaz de Usuario para Calificaciones y Reseñas

## Descripción
Implementa la UI para que clientes creen reseñas y todos los usuarios puedan visualizar reseñas y promedio de calificaciones en el perfil profesional. Incluye formulario intuitivo, sistema de estrellas, validaciones client-side y diseño responsive.

## Estructura de Carpetas
- `/Pages/reviews/ReviewForm.cshtml`: Formulario de nueva reseña.
- `/Pages/reviews/ReviewList.cshtml`: Visualización de lista y promedio de reseñas.
- `/wwwroot/css/reviews/reviews.css`: Estilos de formulario y visualización.
- `/wwwroot/js/reviews/reviewForm.js`: Lógica de interacción de formulario.

## Flujo de Usuario
1. El cliente accede al perfil profesional.
2. Si tiene una reserva completada, puede dejar una reseña usando el formulario modal.
3. Elige calificación (1-5 estrellas) y opcionalmente escribe comentario (máx 500).
4. Envía la reseña; si es exitosa, se muestra mensaje de éxito y la lista se actualiza.
5. Todos los usuarios pueden ver la lista de reseñas y el promedio en el perfil profesional.

## Validaciones
- Calificación obligatoria.
- Comentario opcional, máximo 500 caracteres.
- Solo una reseña por cliente/profesional/booking.
- Mensajes claros de error y éxito.

## Cómo probar
1. Accede como cliente y navega al perfil de un profesional con reserva completada.
2. Abre el formulario de reseña, selecciona estrellas y comenta (opcional).
3. Envía la reseña y verifica el mensaje de éxito.
4. Revisa que la reseña y promedio aparezcan en la lista.
5. Prueba en móvil y escritorio para verificar el diseño responsive.
6. Ejecuta `dotnet build` para validar la integración.

## Mantenimiento
- Para agregar nuevas validaciones o campos, modifica `ReviewForm.cshtml` y su JS.
- Para cambiar la visualización, edita `ReviewList.cshtml` y los estilos CSS.
- La lógica de negocio y endpoints se mantienen en el backend (ver PRO-45).

---
**Responsable:** Equipo ProConnect
**Fecha:** 2025-07-27
