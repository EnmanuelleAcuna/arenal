using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace plani.Models.Domain;

/// <summary>
/// Registro de eventos de una sesión de trabajo (auditoría)
/// </summary>
[Table("SesionLogs")]
public class SesionLog
{
    public SesionLog()
    {
        Id = Guid.NewGuid();
        FechaCreacion = DateTime.UtcNow;
    }

    public SesionLog(Guid idSesion, TipoEventoSesion tipoEvento, DateTime fecha, int horas, int minutos, string creadoPor)
        : this()
    {
        IdSesion = idSesion;
        TipoEvento = tipoEvento;
        Fecha = fecha;
        HorasCalculadas = horas;
        MinutosCalculados = minutos;
        CreadoPor = creadoPor;
    }

    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Sesion))]
    public Guid IdSesion { get; set; }

    /// <summary>
    /// Sesión a la que pertenece este log
    /// </summary>
    public Sesion Sesion { get; set; }

    /// <summary>
    /// Tipo de evento (Inicio, Pausa, Reanudacion, Finalizacion)
    /// </summary>
    public TipoEventoSesion TipoEvento { get; set; }

    /// <summary>
    /// Fecha y hora UTC del evento
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Horas calculadas desde el evento anterior (0 para Inicio y Reanudación)
    /// </summary>
    public int HorasCalculadas { get; set; }

    /// <summary>
    /// Minutos calculados desde el evento anterior (0 para Inicio y Reanudación)
    /// </summary>
    public int MinutosCalculados { get; set; }

    /// <summary>
    /// Usuario que generó el evento
    /// </summary>
    [StringLength(256)]
    public string CreadoPor { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Descripción legible del tipo de evento
    /// </summary>
    [NotMapped]
    public string TipoEventoDescripcion => TipoEvento switch
    {
        TipoEventoSesion.Inicio => "Inicio",
        TipoEventoSesion.Pausa => "Pausa",
        TipoEventoSesion.Reanudacion => "Reanudación",
        TipoEventoSesion.Finalizacion => "Finalización",
        _ => "Desconocido"
    };

    /// <summary>
    /// Tiempo formateado (ej: "2h 30m")
    /// </summary>
    [NotMapped]
    public string TiempoFormateado =>
        HorasCalculadas > 0 || MinutosCalculados > 0
            ? $"{HorasCalculadas}h {MinutosCalculados}m"
            : "-";
}
