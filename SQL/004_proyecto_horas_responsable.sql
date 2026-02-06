-- =============================================
-- Script: 004_proyecto_horas_responsable.sql
-- Descripcion: Agrega HorasEstimadas y Responsable a Proyectos
-- =============================================

-- Agregar columna HorasEstimadas
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'HorasEstimadas')
BEGIN
    ALTER TABLE Proyectos ADD HorasEstimadas INT NULL;
    PRINT 'Columna HorasEstimadas agregada a Proyectos';
END
GO

-- Agregar columna IdResponsable (FK a Usuarios)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'IdResponsable')
BEGIN
    ALTER TABLE Proyectos ADD IdResponsable NVARCHAR(450) NULL;
    PRINT 'Columna IdResponsable agregada a Proyectos';
END
GO

-- Agregar constraint de FK
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Proyectos_Responsable')
BEGIN
    ALTER TABLE Proyectos
    ADD CONSTRAINT FK_Proyectos_Responsable
    FOREIGN KEY (IdResponsable) REFERENCES Usuarios(Id);
    PRINT 'FK_Proyectos_Responsable creada';
END
GO
