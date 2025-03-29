using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;
using plani.Identity;

namespace plani.Models;

[Table("TiposCliente")]
public class TipoCliente : Base
{
    public TipoCliente() : base()
    {
        Clientes = new List<Cliente>();
    }

    public TipoCliente(Guid id, string nombre, string descripcion) : base()
    {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        Clientes = new List<Cliente>();
    }

    [Key] public Guid Id { get; set; }

    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 50 ? Descripcion.Substring(0, 50) + "..." : Descripcion;

    public ICollection<Cliente> Clientes { get; set; }

    public void Actualizar(TipoCliente tipoCliente, string actualizadoPor)
    {
        Nombre = tipoCliente.Nombre;
        Descripcion = tipoCliente.Descripcion;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("Clientes")]
public class Cliente : Base
{
    public Cliente() : base()
    {
        Contratos = new List<Contrato>();
    }

    public Cliente(Guid id, string identificacion, string nombre, string direccion, string descripcion,
        TipoCliente tipoCliente) : base()
    {
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
        Guid idTipoCliente) : base()
    {
        Id = id;
        Identificacion = identificacion;
        Nombre = nombre;
        Direccion = direccion;
        Descripcion = descripcion;

        IdTipoCliente = idTipoCliente;

        Contratos = new List<Contrato>();
    }

    [Key] public Guid Id { get; set; }

    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [StringLength(250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Dirección")]
    [StringLength(1000, ErrorMessage = "La dirección debe tener máximo 1000 caracteres.")]
    public string Direccion { get; set; }

    [NotMapped]
    public string TruncatedDireccion =>
        Direccion?.Length > 20 ? Direccion.Substring(0, 20) + "..." : Direccion;

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;

    [ForeignKey(nameof(TipoCliente))] public Guid IdTipoCliente { get; set; }
    public TipoCliente TipoCliente { get; set; }

    public ICollection<Contrato> Contratos { get; set; }

    public void Actualizar(Cliente cliente, string actualizadoPor)
    {
        Identificacion = cliente.Identificacion;
        Nombre = cliente.Nombre;
        Direccion = cliente.Direccion;
        Descripcion = cliente.Descripcion;
        IdTipoCliente = cliente.IdTipoCliente;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("Contratos")]
public class Contrato : Base
{
    public Contrato() : base()
    {
        Proyectos = new List<Proyecto>();
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, string descripcion, Guid idCliente,
        Guid idArea) : base()
    {
        Id = id;
        Identificacion = identificacion;
        FechaInicio = fechaInicio;
        Descripcion = descripcion;

        IdCliente = idCliente;
        IdArea = idArea;

        Proyectos = new List<Proyecto>();
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, string descripcion, Cliente cliente,
        Area area) : base()
    {
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
    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [DisplayName("Fecha de inicio")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [NotMapped] public string LongDateFechaInicio => FechaInicio.ToString("D", new CultureInfo("es-ES"));

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;

    [ForeignKey(nameof(Cliente))] public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; }

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    public ICollection<Proyecto> Proyectos { get; set; }

    public void Actualizar(Contrato contrato, string actualizadoPor)
    {
        Identificacion = contrato.Identificacion;
        FechaInicio = contrato.FechaInicio;
        Descripcion = contrato.Descripcion;
        IdCliente = contrato.IdCliente;
        IdArea = contrato.IdArea;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("Proyectos")]
public class Proyecto : Base
{
    public Proyecto() : base()
    {
        Asignaciones = new List<Asignacion>();
    }

    public Proyecto(Guid id, string nombre, DateTime fechaInicio, DateTime fechaFin, string descripcion, Guid idArea,
        Guid idContrato) : base()
    {
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
        Contrato contrato) : base()
    {
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

    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DataType(DataType.Date)] public DateTime FechaInicio { get; set; }
    
    [NotMapped] public string LongDateFechaInicio => FechaInicio.ToString("D", new CultureInfo("es-ES"));

    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }
    
    [NotMapped] public string LongDateFechaFin => FechaFin?.ToString("D", new CultureInfo("es-ES"));

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    [ForeignKey(nameof(Contrato))] public Guid IdContrato { get; set; }
    public Contrato Contrato { get; set; }
    
    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;
    
    public ICollection<Asignacion> Asignaciones { get; set; }

    public void Actualizar(Proyecto proyecto, string actualizadoPor)
    {
        Nombre = proyecto.Nombre;
        FechaInicio = proyecto.FechaInicio;
        FechaFin = proyecto.FechaFin;
        Descripcion = proyecto.Descripcion;
        IdArea = proyecto.IdArea;
        IdContrato = proyecto.IdContrato;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("Asignaciones")]
public class Asignacion : Base
{
    public Asignacion() : base()
    {
    }

    public Asignacion(Guid id, int horasEstimadas, string descripcion, Guid idProyecto, string idColaborador) : base()
    {
        Id = id;
        HorasEstimadas = horasEstimadas;
        Descripcion = descripcion;

        IdProyecto = idProyecto;
        IdColaborador = idColaborador;
    }
    
    public Asignacion(Guid id, int horasEstimadas, string descripcion, Proyecto proyecto, ApplicationUser usuario) : base()
    {
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

    [DisplayName("Horas estimadas")]
    public int HorasEstimadas { get; set; }
    
    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;
    
    public void Actualizar(Asignacion asignacion, string actualizadoPor)
    {
        HorasEstimadas = asignacion.HorasEstimadas;
        Descripcion = asignacion.Descripcion;
        IdProyecto = asignacion.IdProyecto;
        IdColaborador = asignacion.IdColaborador;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("Sesiones")]
public class Sesion : Base
{
    public Sesion() : base()
    {
    }

    public Sesion(Guid id, DateTime fechaInicio, int horas, string descripcion, Guid idProyecto, string idColaborador) : base()
    {
        Id = id;
        FechaInicio = fechaInicio;
        Horas = horas;
        Descripcion = descripcion;

        IdProyecto = idProyecto;
        IdColaborador = idColaborador;
    }
    
    public Sesion(Guid id, DateTime fechaInicio, int horas, string descripcion, Proyecto proyecto, ApplicationUser usuario) : base()
    {
        Id = id;
        FechaInicio = fechaInicio;
        Horas = horas;
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
    
    // [DataType(DataType.Date)]
    [Column("Fecha")]
    public DateTime FechaInicio { get; set; }
    
    [Column("FechaPausa")]
    public DateTime? FechaPausa { get; set; }
    
    // [DataType(DataType.Date)] 
    public DateTime? FechaFin { get; set; }

    [DisplayName("Horas")]
    [Column(TypeName = "NUMERIC(18,2)")]
    public double Horas { get; set; }
    
    [ForeignKey(nameof(Servicio))] public Guid IdServicio { get; set; }
    public Servicio Servicio { get; set; }
    
    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;
    
    public void Actualizar(Sesion sesion, string actualizadoPor)
    {
        FechaInicio = sesion.FechaInicio;
        Horas = sesion.Horas;
        Descripcion = sesion.Descripcion;
        IdProyecto = sesion.IdProyecto;
        IdColaborador = sesion.IdColaborador;
        IdServicio = sesion.IdServicio;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
