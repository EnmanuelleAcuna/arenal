using Microsoft.Extensions.Logging.Abstractions;
using plani.Models.Managers;

namespace plani.Tests;

public class AreasManagerTests
{
    /// <summary>
    /// ServiciosController/DetalleArea depende de que ObtenerDetalleAsync cargue Servicios;
    /// Contratos -> Cliente; y Proyectos -> Contrato -> Cliente (igual que el query directo a _dbContext que reemplazó).
    /// </summary>
    [Fact]
    public async Task ObtenerDetalleDeArea_DebeIncluirServiciosContratosYProyectos()
    {
        // Arrange
        var bd = new TestDatabase();
        using var seed = bd.NuevoContexto();

        var area = TestSeeder.SeedArea(seed);
        var modalidad = TestSeeder.SeedModalidad(seed);
        TestSeeder.SeedServicio(seed, idArea: area.Id, idModalidad: modalidad.Id);
        var cliente = TestSeeder.SeedCliente(seed);
        var contrato = TestSeeder.SeedContrato(seed, idCliente: cliente.Id, idArea: area.Id);
        TestSeeder.SeedProyecto(seed, idContrato: contrato.Id, idArea: area.Id);
        await seed.SaveChangesAsync();

        // Act
        using var ctx = bd.NuevoContexto(); // contexto fresco
        var manager = new AreasManager(ctx, NullLogger<AreasManager>.Instance);
        var detalle = await manager.ObtenerDetalleAsync(area.Id);

        // Assert
        Assert.NotNull(detalle);

        // Include(a => a.Servicios)
        Assert.Single(detalle!.Servicios);

        // Include(a => a.Contratos).ThenInclude(c => c.Cliente)
        var contratoCargado = Assert.Single(detalle.Contratos);
        Assert.NotNull(contratoCargado.Cliente);
        Assert.Equal(cliente.Id, contratoCargado.Cliente!.Id);

        // Include(a => a.Proyectos).ThenInclude(p => p.Contrato).ThenInclude(c => c.Cliente)
        var proyectoCargado = Assert.Single(detalle.Proyectos);
        Assert.NotNull(proyectoCargado.Contrato);
        Assert.NotNull(proyectoCargado.Contrato!.Cliente);
        Assert.Equal(cliente.Id, proyectoCargado.Contrato.Cliente!.Id);
    }
}
