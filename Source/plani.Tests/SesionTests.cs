using plani.Models.Domain;

namespace plani.Tests;

/// <summary>
/// Tests de la entidad de dominio Sesion. La máquina de estados y el cálculo de tiempo viven en la
/// entidad (rich domain model); el instante "ahora" se inyecta como parámetro, así que los tests son
/// deterministas y ejercitan el CÓDIGO REAL — son la red de seguridad contra el bug de horas infladas.
/// </summary>
public class SesionTests
{
    private static readonly Guid IdProyecto = Guid.NewGuid();
    private static readonly Guid IdServicio = Guid.NewGuid();
    private const string IdColaborador = "user-1";
    private const string Usuario = "test@test.com";

    private static DateTime Utc(int hora, int minuto = 0) => new(2026, 2, 2, hora, minuto, 0, DateTimeKind.Utc);

    private static Sesion IniciarA(int hora, int minuto = 0)
        => Sesion.Iniciar(IdProyecto, IdServicio, "desc", IdColaborador, Usuario, Utc(hora, minuto));

    [Fact]
    public void Pausar_AcumulaElTiempoTranscurridoYQuedaPausada()
    {
        var sesion = IniciarA(8);

        sesion.Pausar("desc", Usuario, Utc(12, 30));

        Assert.Equal(4, sesion.Horas);
        Assert.Equal(30, sesion.Minutes);
        Assert.Equal(EstadoSesion.Pausada, sesion.Estado);
    }

    [Fact]
    public void Finalizar_DespuesDeReanudar_CuentaDesdeLaReanudacion()
    {
        var sesion = IniciarA(8);
        sesion.Pausar("desc", Usuario, Utc(12));        // +4h
        sesion.Reanudar("desc", Usuario, Utc(14));
        sesion.Finalizar("desc", Usuario, Utc(16, 30)); // +2h30 desde la reanudación

        Assert.Equal(6, sesion.Horas);
        Assert.Equal(30, sesion.Minutes);
        Assert.Equal(EstadoSesion.Finalizada, sesion.Estado);
    }

    [Fact]
    public void FlujoCompleto_IniciarPausarReanudarFinalizar_SumaLosTramos()
    {
        var sesion = IniciarA(8);
        sesion.Pausar("desc", Usuario, Utc(12));   // +4h
        sesion.Reanudar("desc", Usuario, Utc(13));
        sesion.Finalizar("desc", Usuario, Utc(17)); // +4h

        Assert.Equal(8, sesion.Horas);
        Assert.Equal(0, sesion.Minutes);
        Assert.Equal(EstadoSesion.Finalizada, sesion.Estado);
    }

    [Fact]
    public void MultiplesPausasYReanudaciones_AcumulanCorrectamente()
    {
        var sesion = IniciarA(8);

        sesion.Pausar("desc", Usuario, Utc(10));        // +2h  => 2h
        sesion.Reanudar("desc", Usuario, Utc(10, 30));
        sesion.Pausar("desc", Usuario, Utc(12, 30));    // +2h  => 4h
        sesion.Reanudar("desc", Usuario, Utc(14));
        sesion.Finalizar("desc", Usuario, Utc(18));     // +4h  => 8h

        Assert.Equal(8, sesion.Horas);
        Assert.Equal(0, sesion.Minutes);
    }

    [Fact]
    public void TiempoTotal_NuncaExcedeElTiempoRealTranscurrido()
    {
        var inicio = Utc(8);
        var sesion = Sesion.Iniciar(IdProyecto, IdServicio, "desc", IdColaborador, Usuario, inicio);

        var tramos = new[]
        {
            (pausa: Utc(10), reanudacion: Utc(10, 30)),
            (pausa: Utc(12), reanudacion: Utc(13)),
            (pausa: Utc(15), reanudacion: Utc(15, 30))
        };

        foreach (var (pausa, reanudacion) in tramos)
        {
            sesion.Pausar("desc", Usuario, pausa);
            sesion.Reanudar("desc", Usuario, reanudacion);
        }

        var fin = Utc(18);
        sesion.Finalizar("desc", Usuario, fin);

        var tiempoRealTranscurrido = (fin - inicio).TotalHours;
        var tiempoRegistrado = sesion.Horas + sesion.Minutes / 60.0;

        Assert.True(tiempoRegistrado <= tiempoRealTranscurrido,
            $"Tiempo registrado ({tiempoRegistrado:F2}h) no debe exceder el real ({tiempoRealTranscurrido:F2}h)");
    }

    [Fact]
    public void TotalDeLogs_CoincideConElTiempoDeLaSesion()
    {
        var sesion = IniciarA(8);
        sesion.Pausar("desc", Usuario, Utc(12));
        sesion.Reanudar("desc", Usuario, Utc(13));
        sesion.Finalizar("desc", Usuario, Utc(16));

        var horasLogs = sesion.Logs.Sum(l => l.HorasCalculadas);
        var minutosLogs = sesion.Logs.Sum(l => l.MinutosCalculados);
        horasLogs += minutosLogs / 60;
        minutosLogs %= 60;

        Assert.Equal(sesion.Horas, horasLogs);
        Assert.Equal(sesion.Minutes, minutosLogs);
    }

    // --- Guardas de transición (invariantes del rich domain) ---

    [Fact]
    public void Pausar_SesionNoActiva_Lanza()
    {
        var sesion = IniciarA(8);
        sesion.Pausar("desc", Usuario, Utc(9)); // queda Pausada

        var ex = Assert.Throws<InvalidOperationException>(() => sesion.Pausar("desc", Usuario, Utc(10)));
        Assert.Equal("Solo puede pausar una sesión activa.", ex.Message);
    }

    [Fact]
    public void Reanudar_SesionNoPausada_Lanza()
    {
        var sesion = IniciarA(8); // Activa

        var ex = Assert.Throws<InvalidOperationException>(() => sesion.Reanudar("desc", Usuario, Utc(9)));
        Assert.Equal("Solo puede reanudar una sesión pausada.", ex.Message);
    }

    [Fact]
    public void Finalizar_SesionPausada_Lanza()
    {
        var sesion = IniciarA(8);
        sesion.Pausar("desc", Usuario, Utc(9));

        var ex = Assert.Throws<InvalidOperationException>(() => sesion.Finalizar("desc", Usuario, Utc(10)));
        Assert.Equal("Debe reanudar la sesión antes de finalizarla.", ex.Message);
    }

    [Fact]
    public void Finalizar_SesionYaFinalizada_Lanza()
    {
        var sesion = IniciarA(8);
        sesion.Finalizar("desc", Usuario, Utc(9));

        var ex = Assert.Throws<InvalidOperationException>(() => sesion.Finalizar("desc", Usuario, Utc(10)));
        Assert.Equal("La sesión ya está finalizada.", ex.Message);
    }
}
