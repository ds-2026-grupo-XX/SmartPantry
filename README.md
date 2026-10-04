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
