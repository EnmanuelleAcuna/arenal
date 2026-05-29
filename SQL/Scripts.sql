SELECT * FROM Usuarios WHERE [Name] LIKE '%LAURA%';

SELECT cli.Id AS IdCliente, cli.Nombre AS NombreCliente, cli.IsDeleted AS ClienteBorrado, cli.DateDeleted AS ClienteBorradoEl,
       c.Id AS IdContrato, c.Identificacion AS NombreContrato,
       p.Id AS IdProyecto, p.Nombre AS NombreProyecto, p.IsDeleted AS ProyectoBorrado,
       s.*
FROM Sesiones s
         INNER JOIN Proyectos p ON s.IdProyecto = p.Id
         INNER JOIN Contratos c ON p.IdContrato = c.Id
         INNER JOIN Clientes cli ON c.IdCliente = cli.Id
WHERE s.Id = '863E496E-B764-4E00-C8D6-08DE8E645752' AND s.IdColaborador = '5b33a5a3-a9ad-4fd6-bd3c-24a4214ee1d3'
ORDER BY DateCreated DESC;

UPDATE Proyectos
SET IsDeleted = 1, DeletedBy = 'contabilidad@sandiconsultores.com', DateDeleted = GETUTCDATE()
WHERE Id = '554B6C98-3937-42AD-0BCB-08DE8E66B250';

UPDATE Sesiones
SET IsDeleted = 1, DeletedBy = 'contabilidad@sandiconsultores.com', DateDeleted = GETUTCDATE()
WHERE Id = '863E496E-B764-4E00-C8D6-08DE8E645752';

UPDATE Contratos
SET IsDeleted = 1, DeletedBy = 'contabilidad@sandiconsultores.com', DateDeleted = GETUTCDATE()
WHERE Id = '7EF21C0A-6EB4-43BD-88F0-08DE8E6690E2';

SET ANSI_NULLS OFF;
-- 3-102-945101 SRL SOC MAJIT/CANADIENSE
-- Id: 554B6C98-3937-42AD-0BCB-08DE8E66B250
SELECT * FROM Proyectos WHERE Nombre LIKE '%3-102-945101 SRL SOC MAJIT/CANADIENSE%';

-- Id 554B6C98-3937-42AD-0BCB-08DE8E66B250
SELECT * FROM Proyectos WHERE Id = '554B6C98-3937-42AD-0BCB-08DE8E66B250';

SELECT * from Contratos WHERE Id = '7EF21C0A-6EB4-43BD-88F0-08DE8E6690E2';

SELECT * FROM Clientes WHERE Id = '4A409A97-5611-4A5F-4DB6-08DE8E6690E2';

UPDATE Sesiones SET Estado = 3, FechaFin = Fecha WHERE Id = '863E496E-B764-4E00-C8D6-08DE8E645752';

SELECT s.*
FROM Sesiones s
WHERE s.IdProyecto = '554B6C98-3937-42AD-0BCB-08DE8E66B250'
ORDER BY s.DateCreated DESC;