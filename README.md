# plani

Plataforma para la gestión de trabajo mediante el control de sesiones de trabajo, proyectos y colaboradores, para Sandí Consultores.

Gestiona la estructura organizacional del recurso humano (colaboradores, áreas, servicios) y el trabajo por cliente: clientes (empresas), contratos, proyectos y registro de sesiones de trabajo con las horas empleadas en cada servicio.

## Arquitectura

Monolito **ASP.NET Core MVC (.NET)** de proyecto único, en **capas (N-layer)** con **patrón Manager**
(los Managers combinan lógica de negocio y acceso a datos sobre EF Core) y un núcleo de dominio en
enriquecimiento progresivo. **No es Clean Architecture.**

Referencia completa: **[`Documentation/ARQUITECTURA.md`](Documentation/ARQUITECTURA.md)**.

- **Front end**: ASP.NET Core MVC, Razor, jQuery, Bootstrap, DataTables.
- **Back end**: ASP.NET Core MVC, Entity Framework Core, SQL Server (Azure).

## Getting Started

1. [Proceso de instalación](#proceso-instalacion)
2. [Dependencias de software](#dependencias-software)
3. [Pruebas](#pruebas)
4. [Base de datos](#base-de-datos)
5. [Contribuir](#contribuir)

<h2 id="proceso-instalacion">Proceso de instalación</h2>

```bash
# Restaurar dependencias
dotnet restore Source/

# Compilar
dotnet build Source/ --no-restore

# Ejecutar
dotnet run --project Source/plani --no-build
```

Los secretos (cadena de conexión, credenciales SMTP) se configuran vía user-secrets en desarrollo y
variables de entorno en producción — no se commitean.

<h2 id="dependencias-software">Dependencias de software</h2>

- .NET SDK (ver `Source/plani/plani.csproj` para el `TargetFramework` vigente).
- SQL Server (producción: Azure SQL).

<h2 id="pruebas">Pruebas</h2>

```bash
dotnet test Source/
```

Pruebas con xUnit + EF Core InMemory. Convención y patrones en
[`Documentation/ARQUITECTURA.md`](Documentation/ARQUITECTURA.md) §8.

<h2 id="base-de-datos">Base de datos</h2>

Enfoque **database-first**: el esquema vive en scripts SQL numerados en `SQL/`. **No se usan EF migrations.**
Flujo de cambios de esquema en [`Documentation/ARQUITECTURA.md`](Documentation/ARQUITECTURA.md) §9.

<h2 id="contribuir">Contribuir</h2>

- Formato de código (respeta `.editorconfig`):
  ```bash
  dotnet format --severity info
  ```
- LibMan (paquetes del lado del cliente):
  ```bash
  dotnet tool install -g Microsoft.Web.LibraryManager.Cli
  cd Source/plani
  libman restore
  ```
