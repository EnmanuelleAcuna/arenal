# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Plani (Arenal)** is a work session management platform for tracking work sessions, projects, clients, and collaborators. The application is in Spanish and serves organizational resource management for Sandí Consultores.

## Build and Run Commands

```bash
# Restore dependencies
dotnet restore Source/

# Build
dotnet build Source/ --no-restore

# Run the application
dotnet run --project Source/plani --no-build

# Code formatting
dotnet format --severity info

# Client-side libraries (LibMan)
cd Source/plani && libman restore
```

## Architecture

**Stack**: ASP.NET Core MVC monolithic application with SQL Server (Azure)

> Full architecture reference: [`Documentation/ARQUITECTURA.md`](Documentation/ARQUITECTURA.md)

**Key directories** (`Source/plani/`):
- `Controllers/` - MVC controllers inheriting from `BaseController` (HTTP/auth-only helpers; controllers never touch `DbContext`)
- `Models/Domain/` - Domain entities (Base class with soft-delete, audit trail)
- `Models/Managers/` - Business logic managers (one per domain area)
- `Models/ViewModels/` - ViewModels (one file per entity group, constructors accept Domain entities)
- `Models/Data/` - `ApplicationDbContext` and database configuration
- `Identity/` - Custom ASP.NET Identity (ApplicationUser, ApplicationRole, managers)
- `Views/` - Razor views with `_Layout.cshtml` as master layout
- `wwwroot/` - Static assets (Bootstrap 4.6.2, jQuery, DataTables)
- `SQL/` - Database schema scripts (Identity + domain tables)

**Core entities**: Clientes → Contratos → Proyectos → Asignaciones → Sesiones (work sessions)

**Managers** (`Models/Managers/`):
- `ClientesManager` - Clientes, TiposCliente, Contratos (CRUD, validations, dropdowns)
- `ProyectosManager` - Proyectos, Asignaciones (CRUD, validations, email notifications, Excel export)
- `SesionesManager` - Sesiones (real-time sessions, pause/resume/finalize, time calculation, logs)
- `AreasManager` - Áreas (CRUD, validations, dropdowns)
- `ServiciosManager` - Servicios (CRUD, validations, dropdowns)
- `ModalidadesManager` - Modalidades (CRUD, validations)
- `ColaboradoresManager` - Collaborator-specific queries
- `DashboardManager` - Dashboard aggregations

**Patterns**:
- **Manager pattern**: Controllers delegate all business logic and data access to Managers. Controllers do NOT use `_dbContext` directly — use the appropriate Manager instead. Dropdown data comes from each domain manager's `ObtenerParaDropdownAsync` (not from `BaseController`, which is HTTP/auth-only).
- **Soft-delete with validation**: All entities use `IsDeleted` flag. Delete operations validate dependencies (e.g., a Client with active Projects cannot be deleted).
- Audit trail: Entities track `CreatedBy`, `UpdatedBy`, `DeletedBy` with timestamps
- Database-first: No EF migrations; schema in `SQL/` folder
- Fixed Spanish localization (es-ES)
- Active session limit: Users can have a maximum of 2 non-finalized sessions (Activa or Pausada)

## Authentication

Cookie-based ASP.NET Identity with custom managers. Login path: `/Cuentas/IniciarSesion`

Roles: Admin, Coordinador, Colaborador (users can have multiple roles)

## Database

Uses Entity Framework Core (database-first; no EF migrations — schema in `SQL/`). Two DbContexts:
- `ApplicationDbContext` - Main application data
- `IdentityDBContext` - Identity tables

Production connects to Azure SQL Server (`arenal.database.windows.net`)
