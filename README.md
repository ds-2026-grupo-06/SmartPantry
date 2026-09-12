# SmartPantry

## Descripción del proyecto

Esta es una solución modular basada en las prácticas de Domain Driven Design (DDD) provista por ABP Framework. Todos los módulos fundamentales de ABP se encuentran configurados y listos para su uso. Para más información, consultar la documentación de Application Startup Template y Domain Driven Design.

### Estructura de la solución

La aplicación es un monolito en capas compuesto por las siguientes partes principales:
* `angular`: Aplicación frontend en Angular (SPA).
* `SmartPantry.HttpApi.Host`: Backend ASP.NET Core que expone las APIs REST y endpoints de autenticación.
* `SmartPantry.DbMigrator`: Aplicación de consola que aplica las migraciones y realiza el seed de datos inicial.

---

## Requisitos previos

Para clonar, compilar y ejecutar este proyecto en un entorno local se requiere contar con las siguientes herramientas:

* Visual Studio 2022 o 2026 con la carga de trabajo Desarrollo de ASP.NET y web.
* .NET 10.0+ SDK.
* Node.js versión 24.15.0 o superior.
* Yarn versión 1.22.x.
* SQL Server Developer o Express (o LocalDB).
* SQL Server Management Studio (SSMS).
* ABP Studio y ABP CLI (`dotnet tool install -g Volo.Abp.Cli`).
* Git.

---

## Configuración local

La solución requiere configurar la base de datos en los archivos `appsettings.json` de los siguientes dos proyectos:
1. `src/SmartPantry.DbMigrator/appsettings.json`
2. `src/SmartPantry.HttpApi.Host/appsettings.json`

Ambos deben tener declarada la propiedad `ConnectionStrings:Default` con la instancia correspondiente a su entorno local.

### Cadena para LocalDB

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=SmartPantry;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
