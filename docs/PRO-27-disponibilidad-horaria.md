# PRO-27: Configuración de Disponibilidad y Bloqueos - API

## Resumen
Implementación de endpoints REST para que los profesionales configuren sus horarios de disponibilidad, gestionen bloqueos de fechas y consulten su disponibilidad en ProConnect. Esta funcionalidad cumple con los criterios de aceptación del sprint 2 y permite a los clientes conocer cuándo pueden reservar y evitar malentendidos sobre horarios y costos.

---

## ¿Qué hace esta funcionalidad?
Permite a los profesionales:
- Configurar horarios semanales de disponibilidad (lunes a domingo, con horarios por día)
- Definir bloqueos de fechas específicas (vacaciones, eventos, etc.)
- Consultar slots disponibles para una fecha concreta
- Validaciones automáticas de solapamientos y rangos
- Persistencia de toda la configuración en la base de datos MongoDB

---

## Endpoints REST implementados

| Método | Ruta                                             | Descripción                                         | Autenticación |
|--------|--------------------------------------------------|-----------------------------------------------------|---------------|
| PUT    | `/api/professionals/availability/schedule`       | Configurar/actualizar horario semanal               | Professional  |
| POST   | `/api/professionals/availability/block`           | Crear un bloqueo de fechas                          | Professional  |
| GET    | `/api/professionals/availability/blocks`          | Listar todos los bloqueos del profesional           | Professional  |
| DELETE | `/api/professionals/availability/block/{id}`       | Eliminar un bloqueo por id                          | Professional  |
| GET    | `/api/professionals/availability/check?date=YYYY-MM-DD` | Consultar slots disponibles para una fecha          | Professional  |

---

## Funcionamiento y flujo

1. **Configuración de horario semanal**
   - El profesional define su disponibilidad por día (lunes a domingo), indicando si está disponible y el rango horario (formato HH:MM).
   - Se valida que la hora de inicio sea menor a la de fin y que no haya solapamientos.
   - El horario se guarda en el campo `availability_schedule` del documento en la colección `professionalProfiles`.

2. **Gestión de bloqueos**
   - Se pueden crear bloqueos para fechas específicas o rangos (por ejemplo, vacaciones).
   - No se permiten bloqueos solapados.
   - Los bloqueos se almacenan en el campo `availability_blocks` del documento del profesional.

3. **Consulta de disponibilidad**
   - Permite consultar los slots disponibles para una fecha concreta, considerando tanto el horario semanal como los bloqueos.
   - Si la fecha está bloqueada, no se retorna ningún slot disponible.

4. **Persistencia**
   - Toda la configuración se almacena en la colección `professionalProfiles` de MongoDB.
   - Los campos relevantes son `availability_schedule` y `availability_blocks`.

---

## Ubicación del código

- **Controlador:**
  - `Controllers/ProfessionalProfileController.cs`
  - Define los endpoints, validaciones y manejo de errores HTTP.

- **Servicio de aplicación:**
  - `ProConnect.Application/Services/ProfessionalProfileService.cs`
  - Lógica de negocio, validaciones y persistencia.

- **DTOs:**
  - `ProConnect.Application/DTOs/Shared/CommonDtos.cs`
  - DTOs para horarios, bloqueos y consulta de disponibilidad.

- **Repositorio:**
  - `ProConnect.Infrastructure/Repositores/ProfessionalProfileRepository.cs`
  - Acceso a la colección `professionalProfiles` en MongoDB.

- **Entidades:**
  - `ProConnect.Core/Entities/ProfessionalProfile.cs`
  - Estructura de horarios y bloqueos.

---

## Validaciones y seguridad
- Solo usuarios con rol `Professional` pueden acceder a estos endpoints.
- Validación de horarios (inicio < fin, formato HH:MM, no overlapping).
- Validación de bloqueos (no solapados, fechas válidas).
- Control de acceso mediante JWT y `[Authorize(Roles = "Professional")]`.
- Manejo de errores con mensajes claros y códigos HTTP apropiados.

---

## Mantenimiento y futuras actualizaciones
- **Extensión de campos:**
  - Para agregar nuevos tipos de disponibilidad o reglas, actualiza la entidad, DTOs y servicio.
- **Cambios en reglas de negocio:**
  - Modifica la lógica en `ProfessionalProfileService`.
- **Documentación:**
  - Mantén este archivo actualizado ante cualquier cambio relevante.
- **Pruebas:**
  - Se recomienda agregar pruebas unitarias y de integración.

---

## Ejemplo de uso (Swagger)

1. **Autorízate como profesional**
2. Configura tu horario semanal con `PUT /api/professionals/availability/schedule`
3. Agrega un bloqueo con `POST /api/professionals/availability/block`
4. Consulta tus bloqueos con `GET /api/professionals/availability/blocks`
5. Elimina un bloqueo con `DELETE /api/professionals/availability/block/{id}`
6. Consulta tu disponibilidad para una fecha con `GET /api/professionals/availability/check?date=YYYY-MM-DD`

---

## Instrucciones para probar la funcionalidad

1. **Configura tu horario semanal** usando el endpoint correspondiente.
2. **Agrega uno o más bloqueos** y verifica que se reflejen en la base de datos.
3. **Consulta los bloqueos** y verifica que aparecen correctamente.
4. **Elimina un bloqueo** y verifica que desaparece.
5. **Consulta la disponibilidad** para una fecha bloqueada (debe regresar slots vacíos) y para una fecha disponible (debe mostrar los slots configurados).
6. **Verifica en MongoDB Compass** que los cambios se reflejan en los campos `availability_schedule` y `availability_blocks` del documento del profesional.

---

## Contacto y soporte
Para dudas o soporte sobre esta funcionalidad, contactar al equipo de backend o revisar la documentación técnica en el repositorio. 