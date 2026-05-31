using plani.Models.Managers;

namespace plani.Tests;

public class ColaboradoresManagerTests
{
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
}
