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

{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=SmartPantry;Trusted_Connection=True;TrustServerCertificate=True"
  }
}

### Cadena para SQL Server Express

{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=SmartPantry;Trusted_Connection=True;TrustServerCertificate=True"
  }
}

> Manejo seguro de credenciales:
> Si se utiliza un servidor con usuario y contraseña, no se deben subir credenciales al repositorio. Se debe utilizar el gestor de secretos de .NET (User Secrets) o la variable de entorno `ConnectionStrings__Default`:
> dotnet user-secrets set "ConnectionStrings:Default" "<tu_conexion>" --project src/SmartPantry.HttpApi.Host
> dotnet user-secrets set "ConnectionStrings:Default" "<tu_conexion>" --project src/SmartPantry.DbMigrator

---

## Puesta en marcha

Seguir estos pasos en orden para inicializar y correr la aplicación:

### 1. Restaurar dependencias del backend y librerías cliente
Desde la raíz del repositorio, restaurar paquetes de NuGet e instalar las librerías web requeridas por ABP:
dotnet restore ./SmartPantry.slnx
cd src/SmartPantry.HttpApi.Host
abp install-libs
cd ../..

### 2. Instalar dependencias del frontend (Angular)
cd angular
yarn install
cd ..

### 3. Migrar y poblar la base de datos (DbMigrator)
Ejecutar el proyecto de migraciones para crear las tablas y aplicar los datos base del sistema:
* Desde Visual Studio: Establecer `SmartPantry.DbMigrator` como proyecto de inicio y presionar `F5`.
* Desde terminal:
dotnet run --project src/SmartPantry.DbMigrator

Esperar hasta que finalice con el mensaje `Successfully completed all database migrations.`.

### 4. Iniciar el Backend (HttpApi.Host)
* Desde Visual Studio: Establecer `SmartPantry.HttpApi.Host` como proyecto de inicio y ejecutar (`Ctrl + F5` o `F5`).
* Desde terminal:
dotnet run --project src/SmartPantry.HttpApi.Host

### 5. Iniciar la aplicación Angular
En una terminal situada en la carpeta `angular`:
cd angular
yarn start

---

## URLs locales de la aplicación

* Frontend (Angular): http://localhost:4200
* Backend API & Swagger UI: https://localhost:44303/swagger
* Servidor de autenticación (OpenIddict): https://localhost:44303

---

## Verificación de compilación y pruebas

Para comprobar que los proyectos compilan y pasan todos los tests (replicando el flujo validado en el pipeline de CI):

### Backend (.NET)
dotnet build ./SmartPantry.slnx --configuration Release
dotnet test ./SmartPantry.slnx --configuration Release --no-build

### Frontend (Angular)
cd angular
yarn build
yarn test --watch=false

---

## Recursos adicionales de ABP

* [Guía del Frontend Angular](./angular/README.md)
* [Documentación oficial de ABP Framework](https://abp.io/docs/latest)
* [Configuración de certificados OpenIddict en producción](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment)
* [Guía de despliegue de ABP](https://abp.io/docs/latest/Deployment/Index)
