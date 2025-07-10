# README.md

## Instrucciones de instalación

1. Clonar el repositorio  
   ```bash
   git clone 
   cd ProConnect
   ```
2. Restaurar paquetes NuGet  
   ```bash
   dotnet restore
   ```
3. Configurar variables de entorno  
   - Copiar `appsettings.example.json` a `appsettings.json`  
   - Rellenar cadena de conexión a MongoDB en `ConnectionStrings:DefaultConnection`  
   - Agregar `Jwt:Key`, `Jwt:Issuer` y `Jwt:Audience`  
4. Iniciar MongoDB (local o en Docker)  
   ```bash
   # Si usas Docker:
   docker run --name mongo-proconnect -p 27017:27017 -d mongo:latest
   ```
5. Compilar proyecto  
   ```bash
   dotnet build
   ```

## Comandos básicos de desarrollo

- Ejecutar el servidor Web API  
  ```bash
  dotnet run --project ProConnect.Web
  ```
- Ejecutar en modo Debug (Visual Studio Code)  
  1. Abrir carpeta en VS Code  
  2. Seleccionar “.NET Core Launch (web)” en el panel de Debug  
  3. Presionar F5
- Formatear código con dotnet-format  
  ```bash
  dotnet tool install --global dotnet-format
  dotnet format
  ```
- Ejecutar pruebas unitarias  
  ```bash
  dotnet test ProConnect.UnitTests
  ```
- Crear nueva rama de característica  
  ```bash
  git checkout develop
  git checkout -b feature/nombre-de-la-tarea
  ```

## Estructura del proyecto

```
ProConnect/
│
├─ ProConnect.Core/               # Núcleo del dominio
│  ├─ Entities/                   # Entidades del modelo
│  ├─ Interfaces/                 # Contratos (repositorios, servicios)
│  └─ Models/                     # DTOs de dominio
│
├─ ProConnect.Application/        # Lógica de aplicación
│  ├─ UseCases/                   # Casos de uso
│  ├─ Services/                   # Implementaciones de casos de uso
│  └─ DTOs/                       # Objetos de transferencia
│
├─ ProConnect.Infrastructure/     # Implementación técnica
│  ├─ Data/                       # Contexto y mapeos MongoDB
│  ├─ Repositories/               # Patrón Repository
│  ├─ Logging/                    # Configuración de Serilog
│  └─ Services/                   # Integración con servicios externos
│
├─ ProConnect.Web/                # API Web ASP.NET Core
│  ├─ Controllers/                # Endpoints REST
│  ├─ Configuration/              # Extensiones de IServiceCollection
│  ├─ Middleware/                 # Autenticación, autorización, errores
│  └─ wwwroot/                    # Recursos estáticos (CSS, JS, imágenes)
│
├─ ProConnect.UnitTests/          # Pruebas unitarias
├─ ProConnect.IntegrationTests/   # Pruebas de integración
├─ ProConnect.E2ETests/           # Pruebas end-to-end
│
├─ .gitignore
├─ README.md
└─ ProConnect.sln
```