using plani.Models.Domain;
using plani.Models.Managers;

namespace plani.Tests;

public class ColaboradoresManagerTests
{
    private static readonly DateTime FechaBase = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// CuentasController/DetalleColaborador depende de que ObtenerDetalleAsync cargue la cadena
    /// Asignaciones -> Proyecto -> Contrato -> Cliente (igual que el query directo a _dbContext que reemplazó).
    /// </summary>
    [Fact]
    public async Task ObtenerDetalleDeColaborador_DebeIncluirAsignacionConProyectoContratoYCliente()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();

        var area = TestSeeder.SeedArea(seed);
        var cliente = TestSeeder.SeedCliente(seed);
        var contrato = TestSeeder.SeedContrato(seed, idCliente: cliente.Id, idArea: area.Id);
        var proyecto = TestSeeder.SeedProyecto(seed, idContrato: contrato.Id, idArea: area.Id);
        var colaborador = TestSeeder.SeedColaborador(seed);
        TestSeeder.SeedAsignacion(seed, idProyecto: proyecto.Id, idColaborador: colaborador.Id);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto(); // contexto fresco
        var manager = new ColaboradoresManager(ctx);
        var detalle = await manager.ObtenerDetalleAsync(colaborador.Id);

        // Assert
        Assert.NotNull(detalle);
        var asignacion = Assert.Single(detalle!.Asignaciones);
        Assert.NotNull(asignacion.Proyecto);
        Assert.NotNull(asignacion.Proyecto!.Contrato);
        Assert.NotNull(asignacion.Proyecto.Contrato!.Cliente);
        Assert.Equal(cliente.Id, asignacion.Proyecto.Contrato.Cliente!.Id);
    }

    /// <summary>
    /// Un colaborador sin asignaciones, sesiones ni proyectos a cargo debe poder eliminarse.
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_SinDependencias_DebeRetornarNull()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var colaborador = TestSeeder.SeedColaborador(seed);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Null(error);
    }

    /// <summary>
    /// CuentasController/EliminarUsuario (POST) depende de esta validación: eliminar un colaborador
    /// con asignaciones activas deja filas cuyo Include de ApplicationUser resuelve a null (el filtro
    /// global de soft-delete lo excluye) y /Clientes/Asignaciones lanza NullReferenceException.
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_ConAsignacionActiva_DebeRetornarError()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var colaborador = TestSeeder.SeedColaborador(seed);
        TestSeeder.SeedAsignacion(seed, idProyecto: Guid.NewGuid(), idColaborador: colaborador.Id);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Equal("No se puede eliminar el usuario porque tiene asignaciones activas", error);
    }

    /// <summary>
    /// Una asignación ya eliminada (soft-delete) no debe bloquear la eliminación del colaborador.
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_ConAsignacionEliminada_DebeRetornarNull()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var colaborador = TestSeeder.SeedColaborador(seed);
        var asignacion = TestSeeder.SeedAsignacion(seed, idProyecto: Guid.NewGuid(), idColaborador: colaborador.Id);
        asignacion.Eliminar(eliminadoPor: "test");
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Null(error);
    }

    /// <summary>
    /// Un colaborador con una sesión Activa o Pausada no debe poder eliminarse:
    /// la sesión quedaría huérfana y nadie podría finalizarla.
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_ConSesionSinFinalizar_DebeRetornarError()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var colaborador = TestSeeder.SeedColaborador(seed);
        var sesion = new Sesion(Guid.NewGuid(), FechaBase, 0, 0, "Sesión en curso",
            idProyecto: Guid.NewGuid(), idColaborador: colaborador.Id) {
            Estado = EstadoSesion.Activa
        };
        seed.Sesiones.Add(sesion);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Equal("No se puede eliminar el usuario porque tiene sesiones activas o pausadas", error);
    }

    /// <summary>
    /// Las sesiones finalizadas son registro histórico de horas: no deben bloquear la eliminación
    /// (las vistas muestran "Colaborador eliminado" para esas sesiones).
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_ConSesionFinalizada_DebeRetornarNull()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var colaborador = TestSeeder.SeedColaborador(seed);
        var sesion = new Sesion(Guid.NewGuid(), FechaBase, 2, 30, "Sesión terminada",
            idProyecto: Guid.NewGuid(), idColaborador: colaborador.Id) {
            Estado = EstadoSesion.Finalizada,
            FechaFin = FechaBase.AddHours(2.5)
        };
        seed.Sesiones.Add(sesion);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Null(error);
    }

    /// <summary>
    /// Un colaborador responsable de un proyecto activo no debe poder eliminarse:
    /// el IdResponsable quedaría colgando y el proyecto aparecería sin responsable.
    /// </summary>
    [Fact]
    public async Task ValidarEliminacion_SiEsResponsableDeProyecto_DebeRetornarError()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();
        var area = TestSeeder.SeedArea(seed);
        var cliente = TestSeeder.SeedCliente(seed);
        var contrato = TestSeeder.SeedContrato(seed, idCliente: cliente.Id, idArea: area.Id);
        var proyecto = TestSeeder.SeedProyecto(seed, idContrato: contrato.Id, idArea: area.Id);
        var colaborador = TestSeeder.SeedColaborador(seed);
        proyecto.IdResponsable = colaborador.Id;
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto();
        var manager = new ColaboradoresManager(ctx);
        var error = await manager.ValidarEliminacionAsync(colaborador.Id);

        // Assert
        Assert.Equal("No se puede eliminar el usuario porque es responsable de proyectos activos", error);
    }
}
