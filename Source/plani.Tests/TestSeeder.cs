using plani.Identity;
using plani.Models.Data;
using plani.Models.Domain;

namespace plani.Tests;

/// <summary>
/// Helpers de seeding para tests. Cada método crea UNA entidad con valores por defecto razonables,
/// la agrega al contexto y la devuelve (NO hace SaveChanges: el test guarda una sola vez al final).
///
/// Convención de parámetros:
///   - el <see cref="ApplicationDbContext"/> donde seedear (requerido),
///   - las FKs necesarias para conectar el grafo (requeridas),
///   - overrides opcionales con default (id, nombre, ...).
/// Devolver la entidad permite al test leer su Id para encadenar y para las aserciones.
/// </summary>
internal static class TestSeeder
{
    private static readonly DateTime FechaBase = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Area SeedArea(ApplicationDbContext ctx, Guid? id = null, string nombre = "Área de prueba")
    {
        var area = new Area(id ?? Guid.NewGuid(), nombre, "Descripción de prueba");
        ctx.Areas.Add(area);
        return area;
    }

    public static Modalidad SeedModalidad(ApplicationDbContext ctx, Guid? id = null, string nombre = "Modalidad de prueba")
    {
        var modalidad = new Modalidad(id ?? Guid.NewGuid(), nombre, "Descripción de prueba");
        ctx.Modalidades.Add(modalidad);
        return modalidad;
    }

    public static Servicio SeedServicio(ApplicationDbContext ctx, Guid idArea, Guid idModalidad,
        Guid? id = null, string nombre = "Servicio de prueba")
    {
        var servicio = new Servicio(id ?? Guid.NewGuid(), nombre, "Descripción de prueba", idArea, idModalidad);
        ctx.Servicios.Add(servicio);
        return servicio;
    }

    public static Cliente SeedCliente(ApplicationDbContext ctx, Guid? id = null,
        Guid? idTipoCliente = null, string nombre = "Cliente de prueba")
    {
        var cliente = new Cliente(id ?? Guid.NewGuid(), "C-001", nombre, "Dirección", "Descripción",
            idTipoCliente ?? Guid.NewGuid());
        ctx.Clientes.Add(cliente);
        return cliente;
    }

    public static Contrato SeedContrato(ApplicationDbContext ctx, Guid idCliente, Guid idArea, Guid? id = null)
    {
        var contrato = new Contrato(id ?? Guid.NewGuid(), "CON-001", FechaBase, "Descripción", idCliente, idArea);
        ctx.Contratos.Add(contrato);
        return contrato;
    }

    public static Proyecto SeedProyecto(ApplicationDbContext ctx, Guid idContrato, Guid idArea,
        Guid? id = null, string nombre = "Proyecto de prueba")
    {
        var proyecto = new Proyecto(id ?? Guid.NewGuid(), nombre, FechaBase, FechaBase.AddMonths(5),
            "Descripción", idArea, idContrato);
        ctx.Proyectos.Add(proyecto);
        return proyecto;
    }

    public static ApplicationUser SeedColaborador(ApplicationDbContext ctx, string? id = null, string nombre = "Colaborador")
    {
        id ??= Guid.NewGuid().ToString();
        var usuario = new ApplicationUser(id, $"{id}@test.com", nombre, "Apellido", "SegundoApellido", "1-1111-1111", true);
        ctx.Usuarios.Add(usuario);
        return usuario;
    }

    public static Asignacion SeedAsignacion(ApplicationDbContext ctx, Guid idProyecto, string idColaborador,
        Guid? id = null, int horasEstimadas = 10)
    {
        var asignacion = new Asignacion(id ?? Guid.NewGuid(), horasEstimadas, "Descripción", idProyecto, idColaborador);
        ctx.Asignaciones.Add(asignacion);
        return asignacion;
    }
}
