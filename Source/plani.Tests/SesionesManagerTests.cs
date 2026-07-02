using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.Managers;
using plani.Models.ViewModels;

namespace plani.Tests;

/// <summary>
/// Tests de orquestación/persistencia del SesionesManager. La lógica de tiempo y de estado se prueba
/// a nivel de entidad en <see cref="SesionTests"/>; acá verificamos que el manager delega en la entidad
/// y que, confiando en el change tracking (sin Update explícito), persiste la sesión y sus logs.
/// </summary>
public class SesionesManagerTests
{
    private static SesionesManager CrearManager(ApplicationDbContext ctx)
        => new(ctx, NullLogger<SesionesManager>.Instance, TimeZoneInfo.Utc);

    [Fact]
    public async Task IniciarYLuegoPausar_PersisteEstadoYLogs()
    {
        var bd = new TestDatabase();
        var model = new AgregarSesionModel
        {
            IdProyecto = Guid.NewGuid(),
            IdServicio = Guid.NewGuid(),
            Descripcion = "Trabajo"
        };

        // Iniciar (un request → su propio contexto)
        using (var ctx = bd.NuevoContexto())
        {
            var (exito, error) = await CrearManager(ctx).IniciarSesion(model, "user-1", "test@test.com");
            Assert.True(exito, error);
        }

        // Se persistió Activa con su log de inicio (contexto fresco)
        Guid idSesion;
        using (var ctx = bd.NuevoContexto())
        {
            var sesion = await ctx.Sesiones.Include(s => s.Logs).SingleAsync();
            idSesion = sesion.Id;
            Assert.Equal(EstadoSesion.Activa, sesion.Estado);
            var log = Assert.Single(sesion.Logs);
            Assert.Equal(TipoEventoSesion.Inicio, log.TipoEvento);
        }

        // Pausar (otro request → otro contexto)
        using (var ctx = bd.NuevoContexto())
        {
            var (exito, error) = await CrearManager(ctx).PausarSesion(idSesion, "pausa", "test@test.com");
            Assert.True(exito, error);
        }

        // Quedó Pausada con el log de pausa persistido
        using (var ctx = bd.NuevoContexto())
        {
            var sesion = await ctx.Sesiones.Include(s => s.Logs).SingleAsync();
            Assert.Equal(EstadoSesion.Pausada, sesion.Estado);
            Assert.Equal(2, sesion.Logs.Count);
            Assert.Contains(sesion.Logs, l => l.TipoEvento == TipoEventoSesion.Pausa);
        }
    }

    [Fact]
    public async Task PausarSesionInexistente_DevuelveError()
    {
        var bd = new TestDatabase();
        using var ctx = bd.NuevoContexto();

        var (exito, error) = await CrearManager(ctx).PausarSesion(Guid.NewGuid(), "x", "test@test.com");

        Assert.False(exito);
        Assert.Equal("Sesión no encontrada.", error);
    }
}
