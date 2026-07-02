using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;
using plani.Identity;

namespace plani.Models.Domain;

[Table("TiposCliente")]
public class TipoCliente : Base {
    public TipoCliente() {
        Clientes = new List<Cliente>();
    }

    public TipoCliente(Guid id, string nombre, string descripcion) {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        Clientes = new List<Cliente>();
    }

    [Key] public Guid Id { get; set; }

    [StringLength(maximumLength: 255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 50 ? Descripcion.Substring(startIndex: 0, length: 50) + "..." : Descripcion;

    public ICollection<Cliente> Clientes { get; set; }

    public void Actualizar(TipoCliente tipoCliente, string actualizadoPor) {
        Nombre = tipoCliente.Nombre;
        Descripcion = tipoCliente.Descripcion;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Clientes")]
public class Cliente : Base {
    public Cliente() {
        Contratos = new List<Contrato>();
    }

    public Cliente(Guid id, string identificacion, string nombre, string direccion, string descripcion,
        TipoCliente tipoCliente) {
        Id = id;
        Identificacion = identificacion;
        Nombre = nombre;
        Direccion = direccion;
        Descripcion = descripcion;

        IdTipoCliente = tipoCliente.Id;
        TipoCliente = tipoCliente;

        Contratos = new List<Contrato>();
    }

    public Cliente(Guid id, string identificacion, string nombre, string direccion, string descripcion,
        Guid idTipoCliente) {
        Id = id;
        Identificacion = identificacion;
        Nombre = nombre;
        Direccion = direccion;
        Descripcion = descripcion;

        IdTipoCliente = idTipoCliente;

        Contratos = new List<Contrato>();
    }

    [Key] public Guid Id { get; set; }

    [StringLength(maximumLength: 100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [StringLength(maximumLength: 250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Dirección")]
    [StringLength(maximumLength: 1000, ErrorMessage = "La dirección debe tener máximo 1000 caracteres.")]
    public string Direccion { get; set; }

    [NotMapped]
    public string TruncatedDireccion =>
        Direccion?.Length > 20 ? Direccion.Substring(startIndex: 0, length: 20) + "..." : Direccion;

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(startIndex: 0, length: 20) + "..." : Descripcion;

    [ForeignKey(nameof(TipoCliente))] public Guid IdTipoCliente { get; set; }
    public TipoCliente TipoCliente { get; set; }

    public ICollection<Contrato> Contratos { get; set; }

    public void Actualizar(Cliente cliente, string actualizadoPor) {
        Identificacion = cliente.Identificacion;
        Nombre = cliente.Nombre;
        Direccion = cliente.Direccion;
        Descripcion = cliente.Descripcion;
        IdTipoCliente = cliente.IdTipoCliente;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Contratos")]
public class Contrato : Base {
    public Contrato() {
        Proyectos = new List<Proyecto>();
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, string descripcion, Guid idCliente,
        Guid idArea) {
        Id = id;
        Identificacion = identificacion;
        FechaInicio = fechaInicio;
        Descripcion = descripcion;

        IdCliente = idCliente;
        IdArea = idArea;

        Proyectos = new List<Proyecto>();
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, string descripcion, Cliente cliente,
        Area area) {
        Id = id;
        Identificacion = identificacion;
        FechaInicio = fechaInicio;
        Descripcion = descripcion;

        IdCliente = cliente.Id;
        Cliente = cliente;

        IdArea = area.Id;
        Area = area;

        Proyectos = new List<Proyecto>();
    }

    public Guid Id { get; set; }

    [DisplayName("Identificación")]
    [StringLength(maximumLength: 100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [DisplayName("Fecha de inicio")]
    [DataType(dataType: DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [NotMapped] public string LongDateFechaInicio => FechaInicio.ToString("D", new CultureInfo("es-ES"));

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(startIndex: 0, length: 20) + "..." : Descripcion;

    [ForeignKey(nameof(Cliente))] public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; }

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    public ICollection<Proyecto> Proyectos { get; set; }

    public void Actualizar(Contrato contrato, string actualizadoPor) {
        Identificacion = contrato.Identificacion;
        FechaInicio = contrato.FechaInicio;
        Descripcion = contrato.Descripcion;
        IdCliente = contrato.IdCliente;
        IdArea = contrato.IdArea;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Proyectos")]
public class Proyecto : Base {
    public Proyecto() {
        Asignaciones = new List<Asignacion>();
    }

    public Proyecto(Guid id, string nombre, DateTime fechaInicio, DateTime fechaFin, string descripcion, Guid idArea,
        Guid idContrato) {
        Id = id;
        Nombre = nombre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Descripcion = descripcion;

        IdArea = idArea;
        IdContrato = idContrato;

        Asignaciones = new List<Asignacion>();
    }

    public Proyecto(Guid id, string nombre, DateTime fechaInicio, DateTime fechaFin, string descripcion, Area area,
        Contrato contrato) {
        Id = id;
        Nombre = nombre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Descripcion = descripcion;

        IdArea = area.Id;
        Area = area;

        IdContrato = contrato.Id;
        Contrato = contrato;

        Asignaciones = new List<Asignacion>();
    }

    public Guid Id { get; set; }

    [StringLength(maximumLength: 255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DataType(dataType: DataType.Date)] public DateTime FechaInicio { get; set; }

    [NotMapped] public string LongDateFechaInicio => FechaInicio.ToString("D", new CultureInfo("es-ES"));

    [DataType(dataType: DataType.Date)] public DateTime? FechaFin { get; set; }

    [NotMapped] public string LongDateFechaFin => FechaFin?.ToString("D", new CultureInfo("es-ES"));

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    [ForeignKey(nameof(Contrato))] public Guid IdContrato { get; set; }
    public Contrato Contrato { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(startIndex: 0, length: 20) + "..." : Descripcion;

    [DisplayName("Horas Estimadas")]
    [Range(minimum: 0, maximum: 10000, ErrorMessage = "Las horas estimadas deben estar entre 0 y 10,000.")]
    public int? HorasEstimadas { get; set; }

    [ForeignKey(nameof(Responsable))] public string IdResponsable { get; set; }

    [DisplayName("Responsable")] public ApplicationUser Responsable { get; set; }

    public ICollection<Asignacion> Asignaciones { get; set; }

    public void Actualizar(Proyecto proyecto, string actualizadoPor) {
        Nombre = proyecto.Nombre;
        FechaInicio = proyecto.FechaInicio;
        FechaFin = proyecto.FechaFin;
        Descripcion = proyecto.Descripcion;
        IdArea = proyecto.IdArea;
        IdContrato = proyecto.IdContrato;
        HorasEstimadas = proyecto.HorasEstimadas;
        IdResponsable = proyecto.IdResponsable;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Asignaciones")]
public class Asignacion : Base {
    public Asignacion() { }

    public Asignacion(Guid id, int horasEstimadas, string descripcion, Guid idProyecto, string idColaborador) {
        Id = id;
        HorasEstimadas = horasEstimadas;
        Descripcion = descripcion;

        IdProyecto = idProyecto;
        IdColaborador = idColaborador;
    }

    public Asignacion(Guid id, int horasEstimadas, string descripcion, Proyecto proyecto,
        ApplicationUser usuario) {
        Id = id;
        HorasEstimadas = horasEstimadas;
        Descripcion = descripcion;

        IdColaborador = usuario.Id;
        ApplicationUser = usuario;

        IdProyecto = proyecto.Id;
        Proyecto = proyecto;
    }

    public Guid Id { get; set; }

    [ForeignKey(nameof(Proyecto))] public Guid IdProyecto { get; set; }
    public Proyecto Proyecto { get; set; }

    [ForeignKey(nameof(ApplicationUser))] public string IdColaborador { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    [DisplayName("Horas estimadas")] public int HorasEstimadas { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(startIndex: 0, length: 20) + "..." : Descripcion;

    public void Actualizar(Asignacion asignacion, string actualizadoPor) {
        HorasEstimadas = asignacion.HorasEstimadas;
        Descripcion = asignacion.Descripcion;
        IdProyecto = asignacion.IdProyecto;
        IdColaborador = asignacion.IdColaborador;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Sesiones")]
public class Sesion : Base {
    public Sesion() {
        Logs = new List<SesionLog>();
    }

    // --- Factories: única vía de creación, la sesión nace siempre en un estado válido ---

    /// <summary>
    ///     Inicia una nueva sesión en tiempo real (estado Activa) y registra su log de inicio.
    /// </summary>
    public static Sesion Iniciar(Guid idProyecto, Guid idServicio, string descripcion,
        string idColaborador, string usuario, DateTime ahora) {
        Sesion sesion = new() {
            Id = Guid.NewGuid(),
            IdProyecto = idProyecto,
            IdServicio = idServicio,
            IdColaborador = idColaborador,
            FechaInicio = ahora,
            Horas = 0,
            Minutes = 0,
            Descripcion = descripcion,
            Estado = EstadoSesion.Activa
        };
        sesion.RegristrarCreacion(creadoPor: usuario, creadoEl: ahora);
        sesion.AgregarLog(tipo: TipoEventoSesion.Inicio, fecha: ahora, horas: 0, minutos: 0, usuario: usuario);
        return sesion;
    }

    /// <summary>
    ///     Crea una sesión manual ya finalizada, con las horas/minutos provistos por el usuario.
    /// </summary>
    public static Sesion CrearManual(Guid idProyecto, Guid idServicio, int horas, int minutos,
        string descripcion, string idColaborador, string usuario, DateTime fecha, DateTime ahora) {
        Sesion sesion = new() {
            Id = Guid.NewGuid(),
            IdProyecto = idProyecto,
            IdServicio = idServicio,
            IdColaborador = idColaborador,
            FechaInicio = fecha,
            FechaFin = fecha,
            Horas = horas,
            Minutes = minutos,
            Descripcion = descripcion,
            Estado = EstadoSesion.Finalizada
        };
        sesion.RegristrarCreacion(creadoPor: usuario, creadoEl: ahora);
        sesion.AgregarLog(tipo: TipoEventoSesion.Inicio, fecha: fecha, horas: 0, minutos: 0, usuario: usuario);
        sesion.AgregarLog(tipo: TipoEventoSesion.Finalizacion, fecha: fecha, horas: horas, minutos: minutos, usuario: usuario);
        return sesion;
    }

    public Guid Id { get; private set; }

    [ForeignKey(nameof(Proyecto))] public Guid IdProyecto { get; private set; }
    public Proyecto Proyecto { get; set; }

    [ForeignKey(nameof(ApplicationUser))] public string IdColaborador { get; private set; }
    public ApplicationUser ApplicationUser { get; set; }

    [Column("Fecha")] public DateTime FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    [DisplayName("Horas")] public int Horas { get; private set; }

    [DisplayName("Minutos")] public int Minutes { get; private set; }

    [ForeignKey(nameof(Servicio))] public Guid IdServicio { get; private set; }
    public Servicio Servicio { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; private set; }

    /// <summary>
    ///     Estado actual de la sesión (Activa, Pausada, Finalizada)
    /// </summary>
    public EstadoSesion Estado { get; private set; } = EstadoSesion.Activa;

    /// <summary>
    ///     Logs de eventos de esta sesión (auditoría)
    /// </summary>
    public ICollection<SesionLog> Logs { get; private set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 50 ? Descripcion.Substring(startIndex: 0, length: 50) + "..." : Descripcion;

    [NotMapped]
    public string EstadoDescripcion => Estado switch {
        EstadoSesion.Activa => "Activa",
        EstadoSesion.Pausada => "Pausada",
        EstadoSesion.Finalizada => "Finalizada",
        _ => "Desconocido"
    };

    // --- Transiciones de estado: cada una protege su invariante ---

    /// <summary>
    ///     Pausa una sesión activa, acumulando el tiempo del tramo desde el último inicio/reanudación.
    /// </summary>
    public void Pausar(string descripcion, string usuario, DateTime ahora) {
        if (Estado != EstadoSesion.Activa) {
            throw new InvalidOperationException(message: "Solo puede pausar una sesión activa.");
        }

        (int horas, int minutos) = CalcularTiempoDesdeUltimoEvento(fechaHasta: ahora);
        AgregarLog(tipo: TipoEventoSesion.Pausa, fecha: ahora, horas: horas, minutos: minutos, usuario: usuario);
        AgregarTiempo(horas: horas, minutos: minutos);

        Estado = EstadoSesion.Pausada;
        Descripcion = descripcion;
        RegistrarActualizacion(actualizadoPor: usuario, actualizadoEl: ahora);
    }

    /// <summary>
    ///     Reanuda una sesión pausada (marca el punto de reinicio del cronómetro).
    /// </summary>
    public void Reanudar(string descripcion, string usuario, DateTime ahora) {
        if (Estado != EstadoSesion.Pausada) {
            throw new InvalidOperationException(message: "Solo puede reanudar una sesión pausada.");
        }

        AgregarLog(tipo: TipoEventoSesion.Reanudacion, fecha: ahora, horas: 0, minutos: 0, usuario: usuario);

        Estado = EstadoSesion.Activa;
        Descripcion = descripcion;
        RegistrarActualizacion(actualizadoPor: usuario, actualizadoEl: ahora);
    }

    /// <summary>
    ///     Finaliza una sesión activa, acumulando el tiempo del último tramo.
    /// </summary>
    public void Finalizar(string descripcion, string usuario, DateTime ahora) {
        if (Estado == EstadoSesion.Finalizada) {
            throw new InvalidOperationException(message: "La sesión ya está finalizada.");
        }

        if (Estado == EstadoSesion.Pausada) {
            throw new InvalidOperationException(message: "Debe reanudar la sesión antes de finalizarla.");
        }

        (int horas, int minutos) = CalcularTiempoDesdeUltimoEvento(fechaHasta: ahora);
        AgregarLog(tipo: TipoEventoSesion.Finalizacion, fecha: ahora, horas: horas, minutos: minutos, usuario: usuario);
        AgregarTiempo(horas: horas, minutos: minutos);

        FechaFin = ahora;
        Estado = EstadoSesion.Finalizada;
        Descripcion = descripcion;
        RegistrarActualizacion(actualizadoPor: usuario, actualizadoEl: ahora);
    }

    // --- Cálculo de tiempo: portado verbatim del manager (corrige el bug de horas infladas) ---

    private (int horas, int minutos) CalcularTiempoDesdeUltimoEvento(DateTime fechaHasta) {
        SesionLog ultimoEventoActivo = Logs
            .Where(l => l.TipoEvento == TipoEventoSesion.Inicio || l.TipoEvento == TipoEventoSesion.Reanudacion)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefault();

        if (ultimoEventoActivo == null) {
            return (0, 0);
        }

        TimeSpan diferencia = fechaHasta - ultimoEventoActivo.Fecha;

        int horas = (int)diferencia.TotalHours;
        int minutos = diferencia.Minutes;

        return (horas, minutos);
    }

    private void AgregarTiempo(int horas, int minutos) {
        Horas += horas;
        Minutes += minutos;

        // Normalizar si los minutos exceden 60
        if (Minutes >= 60) {
            Horas += Minutes / 60;
            Minutes %= 60;
        }
    }

    private void AgregarLog(TipoEventoSesion tipo, DateTime fecha, int horas, int minutos, string usuario) {
        Logs.Add(new SesionLog(idSesion: Id, tipoEvento: tipo, fecha: fecha, horas: horas, minutos: minutos, creadoPor: usuario));
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}