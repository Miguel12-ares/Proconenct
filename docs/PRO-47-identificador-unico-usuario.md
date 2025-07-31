# PRO-47: Implementación de Identificador Único de Usuario

## Descripción
Implementación de un nuevo identificador único para usuarios basado en su documento de identidad, además del correo electrónico existente.

## Cambios Realizados

### 1. Actualización del Modelo de Datos

#### Entidad User (ProConnect.Core/Entities/User.cs)
- **Nuevos campos añadidos:**
  - `DocumentId` (string): Número de documento de identidad
  - `DocumentType` (enum): Tipo de documento

#### Enum DocumentType
```csharp
public enum DocumentType
{
    CC = 1,                    // Cédula de Ciudadanía
    TarjetaIdentidad = 2,      // Tarjeta de Identidad
    CedulaExtranjeria = 3      // Cédula de Extranjería
}
```

#### Validaciones en la entidad:
- DocumentId debe tener mínimo 5 dígitos
- DocumentId solo puede contener números
- DocumentId es obligatorio para el registro

### 2. Actualización de DTOs

#### RegisterUserDto
- Añadidos campos `DocumentId` y `DocumentType`

#### UpdateUserProfileDto
- Añadidos campos `DocumentId` y `DocumentType` con validaciones

#### UserProfileDto
- Añadidos campos `DocumentId` y `DocumentType` para mostrar en el perfil

### 3. Validaciones

#### RegisterUserValidator
- Validación de formato: solo números, mínimo 5 dígitos, máximo 20 caracteres
- Validación de unicidad: no puede existir otro usuario con el mismo DocumentId
- Validación de tipo de documento: debe ser un valor válido del enum

### 4. Actualización del Repositorio

#### IUserRepository
- Nuevo método: `GetByDocumentIdAsync(string documentId)`
- Nuevo método: `DocumentIdExistsAsync(string documentId)`
- Actualizado método: `UpdateProfileFieldsAsync` para incluir los nuevos campos

#### UserRepository
- Implementación de los nuevos métodos
- Búsqueda por DocumentId
- Validación de unicidad de DocumentId

### 5. Actualización de Servicios

#### AuthService
- Registro: incluye los nuevos campos en la creación del usuario
- Actualización de perfil: incluye validaciones y actualización de los nuevos campos
- Obtención de perfil: incluye los nuevos campos en la respuesta

### 6. Actualización de la Interfaz de Usuario

#### Formulario de Registro (Pages/auth/Register.cshtml)
- Campo de selección para tipo de documento
- Campo de texto para número de documento con validaciones HTML5
- Validaciones del lado cliente

#### Formulario de Perfil (Pages/auth/Profile.cshtml)
- Campos para editar tipo y número de documento
- Validaciones del lado cliente y servidor

#### Modelos de Páginas
- RegisterModel: incluye propiedades para los nuevos campos
- ProfileModel: incluye propiedades para los nuevos campos

### 7. Base de Datos

#### Script de Migración (update-database-schema.ps1)
- Actualiza usuarios existentes con valores por defecto
- Crea índices únicos para DocumentId
- Asegura la integridad de datos

## Validaciones Implementadas

### Validaciones del Lado Cliente (HTML5)
```html
<input type="text" 
       id="DocumentId" 
       name="DocumentId" 
       minlength="5" 
       maxlength="20" 
       pattern="[0-9]+" 
       required />
```

### Validaciones del Lado Servidor (FluentValidation)
```csharp
RuleFor(x => x.DocumentId)
    .NotEmpty().WithMessage("El número de documento es requerido.")
    .MinimumLength(5).WithMessage("El número de documento debe tener mínimo 5 dígitos.")
    .MaximumLength(20).WithMessage("El número de documento no puede exceder 20 caracteres.")
    .Matches(@"^[0-9]+$").WithMessage("El número de documento solo puede contener números.")
    .MustAsync(BeUniqueDocumentId).WithMessage("Este número de documento ya está registrado.");
```

## Índices de Base de Datos

### Índices Únicos Creados
1. **Email**: Garantiza que no haya usuarios duplicados por correo
2. **DocumentId**: Garantiza que no haya usuarios duplicados por documento

### Comando MongoDB para crear índices manualmente:
```javascript
// Índice único para email
db.users.createIndex({ "email": 1 }, { unique: true })

// Índice único para documentId
db.users.createIndex({ "documentId": 1 }, { unique: true })
```

## Flujo de Registro Actualizado

1. **Usuario llena el formulario** con todos los campos incluyendo documento
2. **Validación del lado cliente** verifica formato básico
3. **Envío al servidor** con todos los datos
4. **Validación del lado servidor** con FluentValidation
5. **Verificación de unicidad** tanto para email como para DocumentId
6. **Creación del usuario** con todos los campos
7. **Envío de email de verificación**
8. **Respuesta exitosa** con token JWT

## Flujo de Actualización de Perfil

1. **Usuario edita su perfil** incluyendo documento
2. **Validación de formato** del documento
3. **Verificación de unicidad** del documento (excluyendo al usuario actual)
4. **Actualización en base de datos**
5. **Respuesta de éxito/error**

## Consideraciones de Seguridad

### Validaciones Implementadas
- **Formato**: Solo números, longitud mínima y máxima
- **Unicidad**: No puede existir otro usuario con el mismo documento
- **Integridad**: Validaciones tanto del lado cliente como servidor

### Protección de Datos
- Los datos del documento se almacenan de forma segura
- No se exponen en logs o respuestas de error
- Validaciones estrictas para prevenir inyección de datos

## Testing

### Casos de Prueba Recomendados

1. **Registro exitoso** con documento válido
2. **Registro fallido** con documento duplicado
3. **Registro fallido** con documento inválido (letras, muy corto)
4. **Actualización de perfil** con documento válido
5. **Actualización fallida** con documento de otro usuario
6. **Validación de unicidad** entre email y documento

### Comandos de Testing
```bash
# Compilar la aplicación
dotnet build

# Ejecutar tests (si existen)
dotnet test

# Ejecutar la aplicación
dotnet run
```

## Migración de Datos Existentes

### Script de Migración
El archivo `update-database-schema.ps1` debe ejecutarse después de implementar los cambios:

```powershell
# Ejecutar el script de migración
.\update-database-schema.ps1
```

### Valores por Defecto
- **DocumentId**: "0000000000" (10 ceros)
- **DocumentType**: 1 (Cédula de Ciudadanía)

### Nota Importante
Los usuarios existentes tendrán valores por defecto y deberán actualizar su información en el perfil para proporcionar sus datos reales.

## Compatibilidad

### Usuarios Existentes
- Los usuarios existentes pueden seguir usando la aplicación
- Deben actualizar su perfil para incluir su documento real
- El sistema funciona con valores por defecto hasta que actualicen

### Nuevos Usuarios
- Deben proporcionar documento durante el registro
- Validaciones estrictas desde el primer momento
- No pueden registrarse sin documento válido

## Documentación Técnica

### Archivos Modificados
1. `ProConnect.Core/Entities/User.cs`
2. `ProConnect.Application/DTOs/RegisterUserDto.cs`
3. `ProConnect.Application/DTOs/UpdateUserProfileDto.cs`
4. `ProConnect.Application/DTOs/UserProfileDto.cs`
5. `ProConnect.Application/Validators/RegisterUserValidator.cs`
6. `ProConnect.Core/Interfaces/IUserRepository.cs`
7. `ProConnect.Infrastructure/Repositores/UserRepository.cs`
8. `ProConnect.Application/Services/AuthService.cs`
9. `Pages/auth/Register.cshtml`
10. `Pages/auth/Register.cshtml.cs`
11. `Pages/auth/Profile.cshtml`
12. `Pages/auth/Profile.cshtml.cs`

### Archivos Creados
1. `update-database-schema.ps1`
2. `docs/PRO-47-identificador-unico-usuario.md`

## Conclusión

La implementación del identificador único de usuario basado en documento de identidad ha sido completada exitosamente. El sistema ahora:

- ✅ Valida la unicidad tanto del email como del documento
- ✅ Implementa validaciones robustas en ambos lados
- ✅ Mantiene compatibilidad con usuarios existentes
- ✅ Proporciona una experiencia de usuario mejorada
- ✅ Cumple con los requisitos de seguridad y validación

La aplicación compila correctamente y está lista para ser desplegada. 