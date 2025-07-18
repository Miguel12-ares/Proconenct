Diseño y configuración integral de ProConnectDB
A continuación encontrarás la guía completa ―desde la instalación local de MongoDB hasta la implementación del patrón Repository en tu API ASP.NET Core― junto con los diagramas y la documentación formal que necesitarás para mantener tu base de datos limpia, consistente y preparada para escalar.
1. Requisitos previos
Windows 10/11 con privilegios de administrador


.NET 8 SDK instalado


Git y Visual Studio Code (o IDE de tu preferencia)


Conexión a Internet para descargar MongoDB Community Server y MongoDB Compass


2. Modelo de datos ProConnectDB
2.1 Colecciones y documentos principales
El dominio se resuelve en seis colecciones:
users – cuentas de clientes y profesionales


professionalProfiles – CV ampliado del profesional (1:1 con users)


bookings – reservas, agenda y estado de servicio


reviews – calificaciones/reseñas vinculadas a bookings


payments – transacciones y conciliaciones financieras


blogPosts – contenido de divulgación y SEO corporativo



Diagrama de Colecciones y Documentos - Base de Datos ProConnect
2.2 Patrones de embedding
Se embeben subdocumentos que no requieren consulta independiente:
users.profile y users.profile.location


professionalProfiles.experience, pricing, availability


bookings.pricing y meetingDetails



Diagrama de Patrones de Embedding - MongoDB ProConnect
2.3 Relaciones por referencia
Se referencian aquellas entidades con multiplicidad o crecimiento alto:
professionalProfiles.userId → users._id (1 : 1)


bookings.clientId / professionalId → users._id (N : 1)


reviews.bookingId → bookings._id (1 : 1)


payments.bookingId → bookings._id (1 : 1)


blogPosts.authorId → users._id (N : 1)



Diagrama de Referencias y Relaciones - MongoDB ProConnect
2.4 Vista UML de entidades
El diagrama UML resume atributos, métodos de repositorio y multiplicidades para facilitar comunicación con el equipo de desarrollo y QA.

Diagrama UML de Clases - Sistema ProConnect
3. Creación de la base en MongoDB Compass
Inicia MongoDB Compass y conecta a mongodb://localhost:27017.


Click “Create Database” → nombre ProConnectDB, colección inicial users.


En la pestaña Indexes de users agrega un índice único sobre email.


Repite la creación de las demás colecciones siguiendo los nombres del modelo.


Verifica que users, professionalProfiles, bookings, reviews, payments, blogPosts aparezcan con cero documentos iniciales.


4. Definición de esquemas y validación
Aplica reglas $jsonSchema para garantizar consistencia:
Campos obligatorios: email, passwordHash, userType.


Enumeraciones: userType (client|professional), booking.status, payment.status.


Rango numérico: review.rating entre 1 y 5.


Consulta el archivo de especificación completo para copiar/pegar en la consola mongosh y habilitar la validación de cada colección.
documentacion-esquema-proconnect.md
Archivo generado
5. Implementación del patrón Repository en ASP.NET Core
Estructura de proyecto (carpetas Core, Infrastructure, Controllers).


Entidades heredan de BaseEntity con Id, timestamps y atributos BSON.


BsonCollectionAttribute mapea clase → colección.


IBaseRepository<T> define operaciones asíncronas CRUD + filtros.


BaseRepository<T> implementa las operaciones sobre IMongoCollection<T>.


UserRepository añade búsquedas específicas (GetByEmailAsync, EmailExistsAsync).


Revisa los archivos fuente listos para compilar e integrar en tu solución.
BaseRepository.cs
Archivo generado
6. Configuración de la API y conexión
appsettings.json incluye la cadena de conexión y nombre de base de datos.


Inyecta MongoDbSettings, MongoDbContext y IMongoDatabase en Program.cs.


Registra los repositorios con AddScoped para DI.


En el arranque, crea los índices programáticamente (CreateIndexesAsync).


Usa Swagger para probar la ruta api/users y verificar inserciones.


Consulta la guía paso a paso con comandos, checklist de aceptación y buenas prácticas de logging, health-checks y backups.
guia_paso_a_paso_mongodb.md
Archivo generado
7. Prueba de conexión y criterios de aceptación
Ejecuta dotnet run y navega a https://localhost:5001/swagger.


POST /api/users → crea un usuario con correo único.


Observa en consola la traza “Connected to MongoDB” y ID devuelto.


Abre MongoDB Compass → ProConnectDB.users para confirmar el documento.


En Indexes verifica el índice { email: 1 } UNIQUE.


Realiza GET /api/users y confirma que el conteo coincide.


Con ello se cumple:
MongoDB local instalado y en ejecución.


Base ProConnectDB creada.


appsettings.json configurado.


Driver MongoDB.Driver referenciado.


BaseRepository<T> operando con métodos solicitados.


Índice único en users.email.


Logs de conexión exitosos.


DI funcionando para repositorios.


8. Buenas prácticas de mantenimiento
Versiona tus esquemas (campo schemaVersion cuando evolucione un documento).


Automatiza backups diarios con mongodump y rota a S3.


Documenta cada cambio de colección en el archivo Markdown exportado y súbelo al repositorio junto con migraciones.


Usa Compass Schema para verificar desviaciones y Performance Advisor para sugerir índices adicionales.


