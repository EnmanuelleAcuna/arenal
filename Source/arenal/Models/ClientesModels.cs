using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;

namespace arenal.Models;

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
