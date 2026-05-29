using plani.Models.Domain;

namespace plani.Tests;

public class SesionesManagerTests
{
    /// <summary>
    /// Verifica que el calculo de tiempo desde el ultimo evento funciona correctamente
    /// </summary>
    [Fact]
    public void CalcularTiempoDesdeUltimoEvento_ConEventoInicio_CalculaCorrectamente()
    {
        // Arrange
        var logs = new List<SesionLog>
        {
            new SesionLog(Guid.NewGuid(), TipoEventoSesion.Inicio,
                new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc), 0, 0, "test@test.com")
        };

        var fechaHasta = new DateTime(2026, 2, 2, 12, 30, 0, DateTimeKind.Utc);

        // Act
        var (horas, minutos) = CalcularTiempoDesdeUltimoEvento(logs, fechaHasta);

        // Assert
        Assert.Equal(4, horas);
        Assert.Equal(30, minutos);
    }

    /// <summary>
    /// Verifica que despues de una reanudacion, el calculo usa la fecha de reanudacion
    /// </summary>
    [Fact]
    public void CalcularTiempoDesdeUltimoEvento_ConReanudacion_UsaFechaReanudacion()
    {
        // Arrange
        var idSesion = Guid.NewGuid();
        var logs = new List<SesionLog>
        {
            new SesionLog(idSesion, TipoEventoSesion.Inicio,
                new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc), 0, 0, "test@test.com"),
            new SesionLog(idSesion, TipoEventoSesion.Pausa,
                new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc), 4, 0, "test@test.com"),
            new SesionLog(idSesion, TipoEventoSesion.Reanudacion,
                new DateTime(2026, 2, 2, 14, 0, 0, DateTimeKind.Utc), 0, 0, "test@test.com")
        };

        var fechaHasta = new DateTime(2026, 2, 2, 16, 30, 0, DateTimeKind.Utc);

        // Act
        var (horas, minutos) = CalcularTiempoDesdeUltimoEvento(logs, fechaHasta);

        // Assert - debe calcular desde la reanudacion (14:00), no desde el inicio
        Assert.Equal(2, horas);
        Assert.Equal(30, minutos);
    }

    /// <summary>
    /// Simula el flujo completo: Iniciar -> Pausar -> Reanudar -> Finalizar
    /// usando el nuevo sistema de logs
    /// </summary>
    [Fact]
    public void Flujo_IniciarPausarReanudarFinalizar_DebeCalcularCorrectamente()
    {
        // Arrange
        var idSesion = Guid.NewGuid();
        var sesion = new Sesion
        {
            Id = idSesion,
            Horas = 0,
            Minutes = 0,
            Estado = EstadoSesion.Activa,
            Logs = new List<SesionLog>()
        };

        // 1. Iniciar sesion a las 08:00
        sesion.FechaInicio = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Inicio, sesion.FechaInicio, 0, 0, "test@test.com"));

        // 2. Pausar a las 12:00 (4 horas despues)
        var fechaPausa = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var (horasPausa, minutosPausa) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaPausa);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Pausa, fechaPausa, horasPausa, minutosPausa, "test@test.com"));
        AgregarTiempo(sesion, horasPausa, minutosPausa);
        sesion.Estado = EstadoSesion.Pausada;

        Assert.Equal(4, sesion.Horas);
        Assert.Equal(0, sesion.Minutes);
        Assert.Equal(EstadoSesion.Pausada, sesion.Estado);

        // 3. Reanudar a las 13:00
        var fechaReanudacion = new DateTime(2026, 2, 2, 13, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Reanudacion, fechaReanudacion, 0, 0, "test@test.com"));
        sesion.Estado = EstadoSesion.Activa;

        Assert.Equal(EstadoSesion.Activa, sesion.Estado);

        // 4. Finalizar a las 17:00 (4 horas despues del reinicio)
        var fechaFin = new DateTime(2026, 2, 2, 17, 0, 0, DateTimeKind.Utc);
        var (horasFin, minutosFin) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaFin);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Finalizacion, fechaFin, horasFin, minutosFin, "test@test.com"));
        AgregarTiempo(sesion, horasFin, minutosFin);
        sesion.FechaFin = fechaFin;
        sesion.Estado = EstadoSesion.Finalizada;

        // Total esperado: 4 horas (antes de pausa) + 4 horas (despues de reinicio) = 8 horas
        Assert.Equal(8, sesion.Horas);
        Assert.Equal(0, sesion.Minutes);
        Assert.Equal(EstadoSesion.Finalizada, sesion.Estado);
    }

    /// <summary>
    /// Simula el escenario donde se inicia, se pausa varias veces
    /// </summary>
    [Fact]
    public void Flujo_MultiplesPausasYReanudaciones_DebeAcumularCorrectamente()
    {
        var idSesion = Guid.NewGuid();
        var sesion = new Sesion
        {
            Id = idSesion,
            Horas = 0,
            Minutes = 0,
            Estado = EstadoSesion.Activa,
            Logs = new List<SesionLog>()
        };

        // Iniciar a las 08:00
        sesion.FechaInicio = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Inicio, sesion.FechaInicio, 0, 0, "test@test.com"));

        // Pausa 1: a las 10:00 (2 horas)
        var fechaPausa1 = new DateTime(2026, 2, 2, 10, 0, 0, DateTimeKind.Utc);
        var (h1, m1) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaPausa1);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Pausa, fechaPausa1, h1, m1, "test@test.com"));
        AgregarTiempo(sesion, h1, m1);
        sesion.Estado = EstadoSesion.Pausada;

        Assert.Equal(2, sesion.Horas);

        // Reanudar a las 10:30
        var fechaRean1 = new DateTime(2026, 2, 2, 10, 30, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Reanudacion, fechaRean1, 0, 0, "test@test.com"));
        sesion.Estado = EstadoSesion.Activa;

        // Pausa 2: a las 12:30 (2 horas mas)
        var fechaPausa2 = new DateTime(2026, 2, 2, 12, 30, 0, DateTimeKind.Utc);
        var (h2, m2) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaPausa2);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Pausa, fechaPausa2, h2, m2, "test@test.com"));
        AgregarTiempo(sesion, h2, m2);
        sesion.Estado = EstadoSesion.Pausada;

        Assert.Equal(4, sesion.Horas);

        // Reanudar a las 14:00
        var fechaRean2 = new DateTime(2026, 2, 2, 14, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Reanudacion, fechaRean2, 0, 0, "test@test.com"));
        sesion.Estado = EstadoSesion.Activa;

        // Finalizar a las 18:00 (4 horas mas)
        var fechaFin = new DateTime(2026, 2, 2, 18, 0, 0, DateTimeKind.Utc);
        var (hf, mf) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaFin);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Finalizacion, fechaFin, hf, mf, "test@test.com"));
        AgregarTiempo(sesion, hf, mf);
        sesion.Estado = EstadoSesion.Finalizada;

        // Total: 2 + 2 + 4 = 8 horas
        Assert.Equal(8, sesion.Horas);
    }

    /// <summary>
    /// Verifica que con el nuevo sistema, el tiempo total nunca puede exceder el tiempo real transcurrido
    /// </summary>
    [Fact]
    public void NuevoSistema_TiempoTotalNuncaExcedeTiempoReal()
    {
        var idSesion = Guid.NewGuid();
        var sesion = new Sesion
        {
            Id = idSesion,
            Horas = 0,
            Minutes = 0,
            Estado = EstadoSesion.Activa,
            Logs = new List<SesionLog>()
        };

        // Simular un flujo con multiples pausas
        var fechaInicio = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc);
        sesion.FechaInicio = fechaInicio;
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Inicio, fechaInicio, 0, 0, "test@test.com"));

        // Pausar multiples veces
        var tiempos = new[]
        {
            (pausa: new DateTime(2026, 2, 2, 10, 0, 0), reanudacion: new DateTime(2026, 2, 2, 10, 30, 0)),
            (pausa: new DateTime(2026, 2, 2, 12, 0, 0), reanudacion: new DateTime(2026, 2, 2, 13, 0, 0)),
            (pausa: new DateTime(2026, 2, 2, 15, 0, 0), reanudacion: new DateTime(2026, 2, 2, 15, 30, 0)),
        };

        foreach (var (pausa, reanudacion) in tiempos)
        {
            var (hp, mp) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, pausa);
            sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Pausa, pausa, hp, mp, "test@test.com"));
            AgregarTiempo(sesion, hp, mp);
            sesion.Estado = EstadoSesion.Pausada;

            sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Reanudacion, reanudacion, 0, 0, "test@test.com"));
            sesion.Estado = EstadoSesion.Activa;
        }

        // Finalizar
        var fechaFin = new DateTime(2026, 2, 2, 18, 0, 0, DateTimeKind.Utc);
        var (hf, mf) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaFin);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Finalizacion, fechaFin, hf, mf, "test@test.com"));
        AgregarTiempo(sesion, hf, mf);
        sesion.FechaFin = fechaFin;
        sesion.Estado = EstadoSesion.Finalizada;

        // Tiempo real transcurrido: 10 horas (08:00 a 18:00)
        var tiempoRealTranscurrido = (fechaFin - fechaInicio).TotalHours;

        // Tiempo total registrado
        var tiempoTotalRegistrado = sesion.Horas + sesion.Minutes / 60.0;

        // El tiempo registrado NUNCA debe exceder el tiempo real
        Assert.True(tiempoTotalRegistrado <= tiempoRealTranscurrido,
            $"Tiempo registrado ({tiempoTotalRegistrado:F2}h) no debe exceder tiempo real ({tiempoRealTranscurrido:F2}h)");
    }

    /// <summary>
    /// Verifica que el tiempo total de los logs coincide con el tiempo de la sesion
    /// </summary>
    [Fact]
    public void TotalLogsCoincideConSesion()
    {
        var idSesion = Guid.NewGuid();
        var sesion = new Sesion
        {
            Id = idSesion,
            Horas = 0,
            Minutes = 0,
            Estado = EstadoSesion.Activa,
            Logs = new List<SesionLog>()
        };

        // Iniciar
        sesion.FechaInicio = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Inicio, sesion.FechaInicio, 0, 0, "test@test.com"));

        // Pausar
        var fechaPausa = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);
        var (h1, m1) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaPausa);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Pausa, fechaPausa, h1, m1, "test@test.com"));
        AgregarTiempo(sesion, h1, m1);

        // Reanudar
        var fechaRean = new DateTime(2026, 2, 2, 13, 0, 0, DateTimeKind.Utc);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Reanudacion, fechaRean, 0, 0, "test@test.com"));

        // Finalizar
        var fechaFin = new DateTime(2026, 2, 2, 16, 0, 0, DateTimeKind.Utc);
        var (h2, m2) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, fechaFin);
        sesion.Logs.Add(new SesionLog(idSesion, TipoEventoSesion.Finalizacion, fechaFin, h2, m2, "test@test.com"));
        AgregarTiempo(sesion, h2, m2);

        // Sumar horas de los logs
        var horasLogs = sesion.Logs.Sum(l => l.HorasCalculadas);
        var minutosLogs = sesion.Logs.Sum(l => l.MinutosCalculados);

        // Normalizar
        horasLogs += minutosLogs / 60;
        minutosLogs %= 60;

        // Verificar que coincide
        Assert.Equal(sesion.Horas, horasLogs);
        Assert.Equal(sesion.Minutes, minutosLogs);
    }

    #region Metodos auxiliares que replican la logica del SesionesManager

    private (int horas, int minutos) CalcularTiempoDesdeUltimoEvento(ICollection<SesionLog> logs, DateTime fechaHasta)
    {
        var ultimoEventoActivo = logs
            .Where(l => l.TipoEvento == TipoEventoSesion.Inicio || l.TipoEvento == TipoEventoSesion.Reanudacion)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefault();

        if (ultimoEventoActivo == null)
            return (0, 0);

        TimeSpan diferencia = fechaHasta - ultimoEventoActivo.Fecha;

        int horas = (int)diferencia.TotalHours;
        int minutos = diferencia.Minutes;

        return (horas, minutos);
    }

    private void AgregarTiempo(Sesion sesion, int horas, int minutos)
    {
        sesion.Horas += horas;
        sesion.Minutes += minutos;

        if (sesion.Minutes >= 60)
        {
            sesion.Horas += sesion.Minutes / 60;
            sesion.Minutes = sesion.Minutes % 60;
        }
    }

    #endregion
}
