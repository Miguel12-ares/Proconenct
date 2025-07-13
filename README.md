# ProConnect - Sistema de Gestión de Usuarios

## Descripción
ProConnect es una aplicación web ASP.NET Core que proporciona un sistema completo de autenticación y gestión de perfiles de usuario, construida con arquitectura limpia (Clean Architecture).

## Características
- ✅ Autenticación JWT
- ✅ Registro de usuarios con verificación de email
- ✅ Gestión de perfiles de usuario
- ✅ Interfaz web moderna y responsiva
- ✅ Base de datos MongoDB
- ✅ Validación de datos con FluentValidation
- ✅ Arquitectura limpia con separación de capas

## Requisitos Previos
- .NET 9.0 SDK
- MongoDB Community Server
- PowerShell (para scripts de configuración)

## Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone <url-del-repositorio>
cd Proconenct
```

### 2. Configurar MongoDB
Ejecuta el script de configuración de MongoDB como administrador:
```powershell
# Ejecutar como administrador
.\setup-mongodb.ps1
```

O instala MongoDB manualmente:
1. Descarga MongoDB Community Server desde: https://www.mongodb.com/try/download/community
2. Instala MongoDB como servicio
3. Asegúrate de que MongoDB esté ejecutándose en `localhost:27017`

### 3. Restaurar dependencias
```bash
dotnet restore
```

### 4. Compilar el proyecto
```bash
dotnet build
```

### 5. Ejecutar la aplicación
```bash
dotnet run
```

La aplicación estará disponible en:
- **Aplicación web**: http://localhost:5089
- **API Swagger**: http://localhost:5089 (raíz)

## Estructura del Proyecto

```
Proconenct/
├── Controllers/           # Controladores API
├── Pages/                 # Páginas Razor (UI)
├── ProConnect.Core/       # Entidades y interfaces
├── ProConnect.Application/ # Casos de uso y servicios
├── ProConnect.Infrastructure/ # Implementaciones de infraestructura
└── wwwroot/              # Archivos estáticos
```

## Funcionalidades

### Autenticación
- **Registro**: `/auth/Register` - Registro de nuevos usuarios
- **Login**: `/auth/Login` - Inicio de sesión
- **Perfil**: `/auth/Profile` - Gestión del perfil de usuario

### API Endpoints
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Inicio de sesión
- `GET /api/users/profile` - Obtener perfil
- `PUT /api/users/profile` - Actualizar perfil

## Configuración

### Variables de Entorno
El proyecto usa `appsettings.json` y `appsettings.Development.json` para la configuración:

```json
{
  "ConnectionStrings": {
    "MongoConnection": "mongodb://localhost:27017",
    "DatabaseName": "ProConnectDB"
  },
  "JwtSettings": {
    "SecretKey": "tu-clave-secreta",
    "Issuer": "ProConnect.API",
    "Audience": "ProConnect.Client"
  }
}
```

### Configuración de Email
Para el envío de emails de verificación, configura las credenciales SMTP en `appsettings.json`.

## Desarrollo

### Compilación
```bash
dotnet build
```

### Ejecutar tests (si existen)
```bash
dotnet test
```

### Limpiar
```bash
dotnet clean
```

## Solución de Problemas

### MongoDB no se conecta
1. Verifica que MongoDB esté ejecutándose: `mongod --version`
2. Verifica la conexión: `mongo --eval "db.runCommand('ping')"`
3. Asegúrate de que el puerto 27017 esté disponible

### Errores de compilación
1. Limpia la solución: `dotnet clean`
2. Restaura dependencias: `dotnet restore`
3. Recompila: `dotnet build`

### Problemas de autenticación
1. Verifica que las claves JWT estén configuradas
2. Asegúrate de que las cookies estén habilitadas en el navegador
3. Verifica que el token JWT sea válido

## Tecnologías Utilizadas
- **Backend**: ASP.NET Core 9.0
- **Base de Datos**: MongoDB
- **Autenticación**: JWT Bearer Tokens
- **Validación**: FluentValidation
- **Frontend**: Razor Pages, Bootstrap, CSS3
- **Hashing**: BCrypt.Net-Next

## Licencia
Este proyecto está bajo la licencia MIT.

## Contribución
1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request