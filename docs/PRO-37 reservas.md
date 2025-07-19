# PRO-37: Diseño e Implementación del Modelo de Datos para Reservas

## Descripción
Implementación completa del sistema de reservas para la plataforma ProConnect, incluyendo el modelo de datos, repositorios, servicios, controladores y documentación para pruebas.

## Arquitectura Implementada

### 1. Entidad Booking (ProConnect.Core/Entities/Booking.cs)
- **Campos principales:**
  - `Id`: Identificador único de la reserva
  - `ClientId`: ID del cliente que realiza la reserva
  - `ProfessionalId`: ID del profesional
  - `AppointmentDate`: Fecha y hora de la cita
  - `AppointmentDuration`: Duración en minutos
  - `ConsultationType`: Tipo de consulta (VideoCall, InPerson, Chat)
  - `Status`: Estado de la reserva (Pending, Confirmed, Completed, Cancelled)
  - `TotalAmount`: Monto total de la reserva
  - `SpecialNotes`: Notas especiales
  - `CreatedAt`, `UpdatedAt`: Timestamps de auditoría

- **Enums definidos:**
  - `ConsultationType`: VideoCall, InPerson, Chat
  - `BookingStatus`: Pending, Confirmed, Completed, Cancelled

- **Métodos de negocio:**
  - `Confirm()`: Confirma la reserva
  - `Cancel()`: Cancela la reserva
  - `Complete()`: Marca como completada
  - `CanTransitionTo()`: Valida transiciones de estado

### 2. Repositorio (ProConnect.Infrastructure/Repositores/BookingRepository.cs)
- **Métodos implementados:**
  - `CreateAsync()`: Crear nueva reserva
  - `GetByIdAsync()`: Obtener por ID
  - `GetByClientIdAsync()`: Reservas de un cliente
  - `GetByProfessionalIdAsync()`: Reservas de un profesional
  - `UpdateAsync()`: Actualizar reserva
  - `DeleteAsync()`: Eliminar reserva
  - `HasConflictAsync()`: Verificar conflictos de horario
  - `GetByStatusAsync()`: Filtrar por estado
  - `GetByDateRangeAsync()`: Filtrar por rango de fechas
  - `GetUpcomingAsync()`: Próximas reservas

### 3. Servicio de Aplicación (ProConnect.Application/Services/BookingService.cs)
- **Lógica de negocio:**
  - Validación de profesionales activos
  - Verificación de conflictos de horario
  - Cálculo automático de costos
  - Gestión de transiciones de estado
  - Validación de permisos de usuario

- **Métodos principales:**
  - `CreateBookingAsync()`: Crear reserva con validaciones
  - `UpdateBookingAsync()`: Actualizar con permisos
  - `CancelBookingAsync()`: Cancelar con razón opcional
  - `ConfirmBookingAsync()`: Confirmar por profesional
  - `CompleteBookingAsync()`: Marcar como completada
  - `HasConflictAsync()`: Verificar disponibilidad

### 4. DTOs (ProConnect.Application/DTOs/)
- **CreateBookingDto**: Datos para crear reserva
- **BookingDto**: Representación completa de reserva
- **UpdateBookingDto**: Datos para actualizar
- **MeetingDetailsDto**: Detalles de la reunión

### 5. Controlador API (Controllers/BookingController.cs)
- **Endpoints REST:**
  - `POST /api/booking`: Crear reserva
  - `GET /api/booking/{id}`: Obtener por ID
  - `GET /api/booking/client/{clientId}`: Reservas del cliente
  - `GET /api/booking/professional/{professionalId}`: Reservas del profesional
  - `PUT /api/booking/{id}`: Actualizar reserva
  - `DELETE /api/booking/{id}`: Cancelar reserva
  - `POST /api/booking/{id}/confirm`: Confirmar reserva
  - `POST /api/booking/{id}/complete`: Completar reserva
  - `GET /api/booking/conflict`: Verificar conflictos
  - `GET /api/booking/stats`: Estadísticas

### 6. Base de Datos MongoDB
- **Colección**: `Bookings`
- **Índices creados:**
  - `client_id`: Para búsquedas por cliente
  - `professional_id`: Para búsquedas por profesional
  - `appointment_date`: Para búsquedas por fecha
  - `status`: Para filtros por estado
  - Compuesto: `{professional_id: 1, appointment_date: 1}`
  - TTL: Limpieza automática de reservas antiguas

## Instrucciones de Prueba

### 1. Acceder a Swagger
```
https://localhost:7001/swagger
```

### 2. Crear una Reserva
**Endpoint:** `POST /api/booking`

**Body de ejemplo:**
```json
{
  "clientId": "68751c1c2330f9978848b119",
  "professionalId": "6875a099f07a8341e300cdba",
  "consultationType": "Virtual",
  "appointmentDate": "2024-12-25T14:00:00Z",
  "duration": 60,
  "notes": "Consulta sobre desarrollo web",
  "clientPhone": "3112534355",
  "clientEmail": "miguel123.adso@gmail.com"
}
```

**Tipos de consulta válidos:**
- `"Presencial"` - Consulta en persona
- `"Virtual"` - Consulta por videollamada
- `"Telefonica"` - Consulta por teléfono

**Nota importante:** 
- Usa los IDs reales de tu base de datos
- La fecha debe ser futura (ej: `2024-12-25T14:00:00Z`)
- El `professionalId` debe corresponder a un profesional activo
- El `clientId` debe corresponder a un usuario válido

### 3. Obtener Reservas
- **Por ID:** `GET /api/booking/{id}`
- **Por Cliente:** `GET /api/booking/client/{clientId}`
- **Por Profesional:** `GET /api/booking/professional/{professionalId}`

### 4. Gestionar Estados
- **Confirmar:** `POST /api/booking/{id}/confirm`
- **Completar:** `POST /api/booking/{id}/complete`
- **Cancelar:** `DELETE /api/booking/{id}`

### 5. Verificar Conflictos
**Endpoint:** `GET /api/booking/conflict`
**Query Parameters:**
- `professionalId`: ID del profesional
- `appointmentDate`: Fecha de la cita
- `duration`: Duración en minutos

## Validaciones Implementadas

### 1. Validaciones de Negocio
- Profesional debe existir y estar activo
- Fecha no puede estar en el pasado
- No puede haber conflictos de horario
- Solo el cliente o profesional pueden modificar la reserva
- No se pueden modificar reservas completadas o canceladas

### 2. Validaciones de Estado
- Transiciones de estado controladas
- Estados válidos: Pending → Confirmed → Completed
- Cancelación disponible desde cualquier estado

### 3. Validaciones de Datos
- Duración mínima de 15 minutos
- Duración máxima de 480 minutos (8 horas)
- Fecha de cita futura obligatoria
- IDs de cliente y profesional válidos

## Integración con el Sistema

### 1. Dependencias Registradas
- `IBookingRepository` → `BookingRepository`
- `IBookingService` → `BookingService`

### 2. Autenticación y Autorización
- Todos los endpoints requieren autenticación JWT
- Validación de permisos por usuario
- Verificación de propiedad de reserva

### 3. Logging
- Logs informativos para operaciones exitosas
- Logs de error para excepciones
- Trazabilidad completa de operaciones

## Mantenimiento

### 1. Limpieza Automática
- Índice TTL configurado para eliminar reservas antiguas
- Configuración: 365 días después de la fecha de creación

### 2. Monitoreo
- Métricas de reservas por estado
- Estadísticas de uso por profesional
- Alertas para conflictos de horario

### 3. Backup
- Las reservas se incluyen en el backup automático de MongoDB
- Retención configurada según políticas de la empresa

## Consideraciones de Rendimiento

### 1. Índices Optimizados
- Índices compuestos para consultas frecuentes
- Índices TTL para limpieza automática
- Optimización para búsquedas por fecha y profesional

### 2. Paginación
- Implementada en endpoints de listado
- Límite por defecto: 20 registros
- Offset configurable

### 3. Caché
- Preparado para implementación de Redis
- Cache de consultas frecuentes
- Invalidación automática en actualizaciones

## Próximos Pasos

### 1. Funcionalidades Futuras
- Notificaciones por email/SMS
- Integración con calendarios
- Sistema de recordatorios
- Reportes avanzados

### 2. Mejoras Técnicas
- Implementación de eventos de dominio
- Sagas para transacciones complejas
- API GraphQL para consultas complejas
- Webhooks para integraciones externas

## Conclusión

La implementación del sistema de reservas está completa y funcional, siguiendo los principios SOLID y la arquitectura limpia del proyecto. El sistema es escalable, mantenible y está preparado para futuras expansiones. 