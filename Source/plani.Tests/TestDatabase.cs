using Microsoft.EntityFrameworkCore;
using plani.Models.Data;

namespace plani.Tests;

/// <summary>
/// Crea contextos EF Core sobre un store InMemory aislado por instancia.
///
/// Permite el patrón de DOS contextos: seedear con uno y consultar con otro. Es clave para los
/// tests de Include, porque el provider InMemory solo hace fix-up de navegaciones entre entidades
/// trackeadas en el MISMO contexto; con contextos separados, una navegación solo queda cargada si
/// el query tiene un Include real. Sin esto, los tests de Include darían falsos positivos.
/// </summary>
internal sealed class TestDatabase
{
    private readonly DbContextOptions<ApplicationDbContext> _opciones =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    /// <summary>Nuevo DbContext sobre el mismo store en memoria.</summary>
    public ApplicationDbContext NuevoContexto() => new(_opciones);
}
