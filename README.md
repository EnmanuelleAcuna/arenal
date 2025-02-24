# plani
Plataforma para la gestión de trabajo mediante el control de sesiones de trabajo, proyectos y colaboradores.

Gestión de estructura organizacional de el recurso humano de la organizacion, como empleados, areas, jefaturas, tambien de los servicios ofrecidos.

Gestión de proyectos mediante el control de clientes (empresas), proyectos, registro de sesiones de trabajo, mediante la especificacion de horas empleadas ofreciendo los servicios.

## Proceso de planificación y seguimiento de trabajo
Básico: https://docs.microsoft.com/en-us/azure/devops/boards/get-started/plan-track-work?view=azure-devops&tabs=basic-process&source=docs

## Arquitectura de la aplicación: Monolítica
- Se hace uso de Clean architecture para la solución: https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture
- Front end:
  - ASP.Net MVC.
  - JavaScript
  - jQuery
  - Razor
- Back end:
  - ASP.Net MVC.
  - SQL Server.

## Getting Started
1.	[Proceso de instalación](#proceso-instalacion)
2.	[Dependencias de software](#dependencias-software)
3.	[Últimos lanzamientos](#ultimos-lanzamientos)
4.	[Referencias a APIs](#referencias-api)
5.	[Contribuir](#contribuir)

<h2 id="proceso-instalacion">Proceso de instalación</h3>

- Compilar: dotnet build Source/ --no-restore
- Ejecutar: dotnet run Source/Web --no-build

<h2 id="dependencias-software">Dependencias de software</h2>

- dotnet restore Source/ // Restaurar las dependecias del proyecto

<h2 id="ultimos-lanzamientos">Últimos lanzamientos</h2>

- Actualmente en desarrollo de la versión inicial MVP.

<h2 id="referencias-api">Referencias a APIs</h2>
TODO

<h2 id="contribuir">Contribuir</h2>

- Formato de código:
  ```
  dotnet format --severity info
  ```
- LibMan: Administrador de paquetes del lado del cliente
  ```
  // Instalar herramienta
  dotnet tool install -g Microsoft.Web.LibraryManager.Cli

  // Estando en carpeta raiz del repositorio
  cd  Source/Web

  // Restaurar los paquetes que se encuentran en el archivo de configuracion libman.json en Source/Web/
  libman restore
  ```
- Entity Framework Core tools:
  ```
  // Instalar herramienta
  dotnet tool install -g dotnet-ef

  // Verificar dotnet ef, SOLO DESDE LA TERMINAL DE VSCODE O VISUAL STUDIO
  dotnet-ef ó dotnet ef

  // No se utiliza code-first o database-first ni migrations
- dotnet-aspnet-codegenerator:
  ```
  // Instalar
  dotnet tool install -g dotnet-aspnet-codegenerator --version 6.0.11

  // Consultar ayuda de herramienta
  dotnet-aspnet-codegenerator -h

  // Generar las vistas y codigo para las vistas deseadas
  dotnet aspnet-codegenerator identity --dbContext PCG.Data.ApplicationDbContext --files "Account.ForgotPassword;Account.ForgotPasswordConfirmation;Account.Login;Account.Logout;Account.ResetPassword;Account.ResetPasswordConfirmation"
  ```
