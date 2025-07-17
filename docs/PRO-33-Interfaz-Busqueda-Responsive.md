# PRO-33 Interfaz de Búsqueda Responsive

## Descripción
Se implementó una interfaz moderna, intuitiva y completamente responsive para la búsqueda avanzada de profesionales en ProConnect, cumpliendo con los criterios de aceptación del sprint y siguiendo principios SOLID y arquitectura modular.

## Estructura de Archivos
- **/Pages/search/Search.cshtml**: Vista principal de la página de búsqueda.
- **/Pages/search/Search.cshtml.cs**: Code-behind Razor Page (mínimo, lógica en JS).
- **/wwwroot/css/search/main.css**: Estilos generales de la página de búsqueda.
- **/wwwroot/css/search/filters.css**: Estilos para el panel de filtros.
- **/wwwroot/css/search/results.css**: Estilos para las tarjetas de resultados.
- **/wwwroot/js/search/search.js**: Lógica de búsqueda, filtros, renderizado y paginación.

## Funcionamiento
- **Layout**: Grid responsive con Bootstrap 5, panel de filtros lateral (o modal en mobile) y resultados en tarjetas visuales.
- **Barra de búsqueda**: Input principal con autocompletado básico y botón de búsqueda.
- **Filtros**: Especialidad, ubicación, rango de precios, calificación mínima, años de experiencia, consulta virtual. Aplicación inmediata y botón de limpiar.
- **Lógica de combinación de filtros**: Si el usuario escribe en la barra de búsqueda (query), la búsqueda se realiza por texto en bio, especialidades y ubicación. Si además selecciona una especialidad, solo se combinarán ambos filtros si la especialidad es diferente al texto buscado. Si la especialidad seleccionada coincide con el texto, solo se aplica el filtro de texto. Esto evita búsquedas demasiado restrictivas y mejora la experiencia del usuario.
- **Resultados**: Tarjetas con foto, nombre, especialidad, ubicación, experiencia, tarifa, calificación y acceso a perfil.
- **Paginación**: Navegación entre páginas de resultados.
- **Estados**: Mensajes de carga, sin resultados y error.
- **Integración**: Consumo del endpoint `/api/search/professionals` con todos los filtros y paginación.

## Pruebas y Validación
1. **Compilación**: Ejecutar `dotnet build` y verificar que no existan errores.
2. **Acceso**: Navegar a `/search` en la aplicación.
3. **Búsqueda**: Usar la barra de búsqueda y filtros, verificar que los resultados se actualizan en menos de 1 segundo.
4. **Responsive**: Probar en desktop, tablet y móvil. El panel de filtros debe ser colapsable/adaptativo.
5. **Estados**: Probar búsquedas sin resultados y con error (desconectar backend temporalmente).
6. **Swagger**: Validar que el endpoint `/api/search/professionals` responde correctamente con los mismos parámetros que usa el frontend.

## Mantenimiento y Actualización
- Para agregar nuevos filtros, modificar tanto el formulario en `Search.cshtml` como la lógica en `search.js`.
- Los estilos pueden ampliarse en los archivos de la carpeta `/css/search`.
- Para cambiar la lógica de autocompletado, modificar la sección correspondiente en `search.js`.
- El consumo de API puede adaptarse fácilmente a cambios en el backend, ya que los parámetros se construyen dinámicamente.

## Notas
- El código sigue principios SOLID y clean code para facilitar la escalabilidad y el mantenimiento.
- La lógica de combinación de filtros ha sido mejorada para evitar búsquedas demasiado restrictivas: la búsqueda por texto y especialidad ahora se comporta de forma más intuitiva para el usuario.
- Se recomienda revisar las advertencias de compilación, aunque no afectan la funcionalidad del módulo de búsqueda.
- El módulo es completamente independiente y puede integrarse con otros sistemas de navegación o dashboards. 