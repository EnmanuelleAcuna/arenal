CREATE TABLE Areas (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Areas_Id PRIMARY KEY CLUSTERED (Id ASC),
	Nombre NVARCHAR(250) NULL CONSTRAINT UQ_Areas_Nombre UNIQUE,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Areas_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Areas;

EXEC SP_HELP 'Areas';

CREATE TABLE Modalidades (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Modalidades_Id PRIMARY KEY CLUSTERED (Id ASC),
	Nombre NVARCHAR(250) NULL CONSTRAINT UQ_Modalidades_Nombre UNIQUE,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Modalidades_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Modalidades;

EXEC SP_HELP 'Areas';

CREATE TABLE Servicios
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Servicios_Id PRIMARY KEY CLUSTERED, 
	Nombre NVARCHAR(250) NOT NULL CONSTRAINT UQ_Servicios_Nombre UNIQUE,
    Descripcion NVARCHAR(2000) NULL,
    IdArea UNIQUEIDENTIFIER CONSTRAINT FK_Servicios_Areas_IdArea FOREIGN KEY REFERENCES Areas (Id),
    IdModalidad UNIQUEIDENTIFIER CONSTRAINT FK_Servicios_Modalidades_IdModalidad FOREIGN KEY REFERENCES Modalidades (Id),
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Servicios_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Servicios;

CREATE TABLE TiposCliente 
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TiposCliente_Id PRIMARY KEY CLUSTERED (Id ASC),
	Nombre NVARCHAR(250) NULL CONSTRAINT UQ_TiposCliente_Nombre UNIQUE,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_TiposCliente_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM TiposCliente;

CREATE TABLE Clientes
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Clientes_Id PRIMARY KEY CLUSTERED,
    Identificacion NVARCHAR(100) NOT NULL CONSTRAINT UQ_Clientes_Identificacion UNIQUE, 
	Nombre NVARCHAR(250) NULL,
    IdTipoCliente UNIQUEIDENTIFIER CONSTRAINT FK_Clientes_TiposCliente_IdTipoCliente FOREIGN KEY REFERENCES TiposCliente (Id),
    Direccion NVARCHAR(1000) NULL,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Clientes_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Clientes;

CREATE TABLE Contratos
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Contratos_Id PRIMARY KEY CLUSTERED,
    Identificacion NVARCHAR(100) NOT NULL CONSTRAINT UQ_Contratos_Identificacion UNIQUE,
    IdCliente UNIQUEIDENTIFIER CONSTRAINT FK_Contratos_Clientes_IdCliente FOREIGN KEY REFERENCES Clientes (Id),
    IdArea UNIQUEIDENTIFIER CONSTRAINT FK_Contratos_Areas_IdArea FOREIGN KEY REFERENCES Areas (Id),
    FechaInicio DATETIME NOT NULL,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Contratos_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Contratos;

EXEC SP_HELP 'Contratos';

CREATE TABLE Proyectos
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Proyectos_Id PRIMARY KEY CLUSTERED,
    Nombre NVARCHAR(100) NOT NULL,
    IdArea UNIQUEIDENTIFIER CONSTRAINT FK_Proyectos_Areas_IdArea FOREIGN KEY REFERENCES Areas (Id),
    IdContrato UNIQUEIDENTIFIER CONSTRAINT FK_Proyectos_Contratos_IdContrato FOREIGN KEY REFERENCES Contratos (Id),
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NULL,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Proyectos_DateCreated DEFAULT GETDATE(),
	CreatedBy NVARCHAR(100) NOT NULL,
	DateUpdated DATETIME NULL,
	UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
	DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Proyectos;

EXEC SP_HELP 'Proyectos';

CREATE TABLE Asignaciones(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Asignaciones_Id PRIMARY KEY CLUSTERED,
    IdProyecto UNIQUEIDENTIFIER CONSTRAINT FK_Asignaciones_Proyectos_IdProyecto FOREIGN KEY REFERENCES Proyectos (Id),
    IdColaborador NVARCHAR(450) CONSTRAINT FK_Asignaciones_Usuarios_IdUsuario FOREIGN KEY REFERENCES AspNetUsers (Id),
    HorasEstimadas INT NOT NULL,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Asignaciones_DateCreated DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    DateUpdated DATETIME NULL,
    UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
    DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Asignaciones;

CREATE TABLE Sesiones(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Sesiones_Id PRIMARY KEY CLUSTERED,
    IdProyecto UNIQUEIDENTIFIER CONSTRAINT FK_Sesiones_Proyectos_IdProyecto FOREIGN KEY REFERENCES Proyectos (Id),
    IdColaborador NVARCHAR(450) CONSTRAINT FK_Sesiones_Usuarios_IdUsuario FOREIGN KEY REFERENCES AspNetUsers (Id),
    IdServicio UNIQUEIDENTIFIER CONSTRAINT FK_Sesiones_Servicios_IdServicio FOREIGN KEY REFERENCES Servicios (Id),
    Fecha DATETIME NOT NULL,
    Horas NUMERIC(18, 2) NOT NULL,
    Descripcion NVARCHAR(2000) NULL,
    DateCreated DATETIME NOT NULL CONSTRAINT DF_Sesiones_DateCreated DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    DateUpdated DATETIME NULL,
    UpdatedBy NVARCHAR(100) NULL,
    DateDeleted DATETIME NULL,
    DeletedBy NVARCHAR(100) NULL,
    IsDeleted BIT NULL
);

SELECT * FROM Sesiones;

ALTER TABLE Sesiones ADD FechaFin DATETIME NULL;

ALTER TABLE Sesiones ALTER COLUMN Horas NUMERIC(18, 2) NOT NULL;