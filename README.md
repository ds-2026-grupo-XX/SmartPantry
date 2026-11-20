# Desarrollo de Software 2026

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Integrantes

- Aliandre, Matias https://github.com/matiasaliandre
- Caballero, Mario A. https://github.com/marioacaballero
- Coletti, Juan Cruz https://github.com/juancoletti

## Cómo ejecutar

Esta es una solución de inicio en capas basada en las prácticas de Diseño Guiado por el Dominio (DDD)(https://abp.io/docs/latest/framework/architecture/domain-driven-design). Todos los módulos fundamentales de ABP ya están instalados. Consulta la documentación de la Plantilla de Inicio de Aplicación(https://abp.io/docs/latest/solution-templates/layered-web-application) para más información.

### Requisitos

Visual Studio 2022 o 2026 con Desarrollo de ASP.NET y web, Node.js 24.15.0 o superior, Yarn
1.22.x, SQL Server Developer o Express, SQL Server Management Studio (SSMS), ABP Studio y Git.

### Configuración local

La solución incluye una configuración predeterminada que funciona sin necesidad de modificaciones. Sin embargo, es posible que desees cambiar la siguiente configuración antes de ejecutar tu solución:

Revisa los parámetros de ConnectionStrings en los archivos appsettings.json ubicados en los proyectos SmartPantry.HttpApi.Host y SmartPantry.DbMigrator, y cámbialos si lo necesitas.

### Puesta en marcha

Ejecuta el comando abp install-libs en la carpeta raiz para instalar las dependencias.

Ejecuta SmartPantry.DbMigrator para crear la base de datos inicial. Debe realizarse en la primera ejecución y también es necesario si posteriormente se añade una nueva migración de base de datos a la solución.

Para levantar el backend se debe ejecutar el siguiente comando desde la carpeta aspnet-core

- dotnet run --project .\src\MiSolucion.HttpApi.Host\

Para levantar el frontend se debe ejecutar el siguiente comando desde la carpeta angular

- yarn start

### Estructura de la solución

Esta es una aplicación monolítica en capas que consta de las siguientes aplicaciones:

angular: Aplicación de Angular.

SmartPantry.DbMigrator: Una aplicación de consola que aplica las migraciones y también inserta los datos iniciales (seed data). Es útil tanto en el entorno de desarrollo como en el de producción.

SmartPantry.HttpApi.Host: Aplicación de API de ASP.NET Core que se utiliza para exponer las API a los clientes.

### Verificación

Ejecuta los siguientes comando desde el directorio raiz para vericiar que la seccion del backend funciona correctamente

```env
dotnet build ./aspnet-core/SmartPantry.slnx --configuration Release --no-restore
dotnet test ./aspnet-core/SmartPantry.slnx --configuration Release --no-build
```

Ejecuta el siguiente comando desde el directorio angular para correr los test del frontend

```env
yarn test
```

## Mi despensa (RF-11) — TP 10

### Lógica de ítems consumidos
- Según la **Regla de negocio 6**, los ítems marcados como consumidos (`EstadoItemDespensa.Consumido`) no se eliminan automáticamente: se conservan como historial y se muestran diferenciados visualmente en **Mi despensa** (`Disponible` / `Consumido`), ocultando la acción de volver a consumirlos pero manteniendo la trazabilidad y la posibilidad de eliminarlos explícitamente
- Las operaciones de listado (`GetListAsync`), edición (`UpdateAsync`), consumo (`MarkAsConsumedAsync`) y eliminación (`DeleteAsync`) se ejecutan a través de `ItemDespensaAppService` cumpliendo las validaciones de dominio (`ItemDespensa.ActualizarDatos`) y de DTO (`UpdateItemDespensaDto`)

### Uso y mock de `ListService`
- En `MyPantryComponent` se inyecta y utiliza `ListService` (`@abp/ng.core`) mediante `hookToQuery` y `list.get()` porque `ItemDespensaAppService.GetListAsync` es una operación paginada (`PagedResultDto<ItemDespensaDto>`)
- En las pruebas unitarias con **Vitest** (`my-pantry.component.spec.ts`), `ListService` (junto con el proxy `ItemDespensaService`, `ConfirmationService`, `ToasterService`, `LocalizationService` y el router) se reemplaza por mocks para aislar el componente y probar sin dependencia de red los cuatro estados (carga, error, lista vacía y resultados), las validaciones del formulario (cantidad negativa, fecha inválida y longitud máxima de unidad) y la confirmación/cancelación de borrado

### Manejo de fechas y relación con el worker del TP08
- **Criterio de fechas (fecha calendario local):** En el dominio y los DTOs (`ItemDespensa`, `ItemDespensaDto` y `UpdateItemDespensaDto`), `FechaVencimiento` se modela como `DateOnly?` (fecha calendario `YYYY-MM-DD`, sin hora ni zona horaria). En la interfaz Angular se ingresa desde `<input type="date">` como `YYYY-MM-DD`, se valida localmente (`validDateValidator()`) y se formatea según la configuración regional de la aplicación (`date: 'shortDate'`), evitando conversiones a medianoche UTC que desplacen un día en husos horarios locales
- **Relación con el worker del TP08:** `ExpirationWorker` (TP08) continúa ejecutándose en segundo plano de forma autónoma sin usuario conectado y evalúa vencimientos mediante `DateOnly.FromDateTime(DateTime.Today)`, es decir, utilizando **fecha calendario local** del servidor[cite: 4]. De este modo, tanto la pantalla "Mi despensa" como el worker comparten el mismo criterio de fecha calendario (`DateOnly`) sin modificar la regla ni la idempotencia del worker

### Estrategia de manejo de errores y autorización
- **Aislamiento en servidor (`404`):** `ItemDespensaAppService` exige autenticación con `[Authorize]` y resuelve el propietario mediante `ICurrentUser.GetId()` antes de leer o modificar datos. Ante cualquier intento de operar (`PUT`, `POST .../mark-as-consumed` o `DELETE`) sobre un ítem de otro usuario con un token `Bearer` válido, el servidor responde **404 Not Found** (`EntityNotFoundException`) sin revelar si el recurso existe ni modificarlo, y responde **401 Unauthorized** ante peticiones sin sesión ni token.
- **Manejo en interfaz:** Se utiliza el manejo HTTP predeterminado de ABP (sin `skipHandleError` en las llamadas del proxy) complementado con estados de error y notificaciones localizadas (`::Pantry:LoadError` con botón de reintento y `::Pantry:ActionError`), sin exponer detalles técnicos ni códigos HTTP crudos como único mensaje al usuario[cite: 3, 13].