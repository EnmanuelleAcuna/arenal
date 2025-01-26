using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    }

    [Key] public Guid Id { get; set; }

    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [StringLength(250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(1000, ErrorMessage = "La dirección debe tener máximo 1000 caracteres.")]
    public string Direccion { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 50 ? Descripcion.Substring(0, 50) + "..." : Descripcion;

    [ForeignKey(nameof(TipoCliente))] public Guid IdTipoCliente { get; set; }
    public TipoCliente TipoCliente { get; set; }

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
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, Guid idCliente, Guid idArea) : base()
    {
        Id = id;
        Identificacion = identificacion;
        FechaInicio = fechaInicio;

        IdCliente = idCliente;
        IdArea = idArea;
    }

    public Contrato(Guid id, string identificacion, DateTime fechaInicio, Cliente cliente, Area area) : base()
    {
        Id = id;
        Identificacion = identificacion;
        FechaInicio = fechaInicio;

        IdCliente = cliente.Id;
        Cliente = cliente;

        IdArea = area.Id;
        Area = area;
    }

    public Guid Id { get; set; }

    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    public DateTime FechaInicio { get; set; }

    [ForeignKey(nameof(Cliente))] public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; }

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    public void Actualizar(Contrato contrato, string actualizadoPor)
    {
        Identificacion = contrato.Identificacion;
        FechaInicio = contrato.FechaInicio;
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

    public Proyecto(Guid id, string nombre, DateTime fechaInicio, DateTime fechaFin, Guid idArea,
        Guid idContrato) : base()
    {
        Id = id;
        Nombre = nombre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;

        IdArea = idArea;
        IdContrato = idContrato;
    }

    public Proyecto(Guid id, string nombre, DateTime fechaInicio, DateTime fechaFin, Area area,
        Contrato contrato) : base()
    {
        Id = id;
        Nombre = nombre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;

        IdArea = area.Id;
        Area = area;

        IdContrato = contrato.Id;
        Contrato = contrato;
    }

    public Guid Id { get; set; }

    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DataType(DataType.Date)] public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    [ForeignKey(nameof(Contrato))] public Guid IdContrato { get; set; }
    public Contrato Contrato { get; set; }

    public void Actualizar(Proyecto proyecto, string actualizadoPor)
    {
        Nombre = proyecto.Nombre;
        FechaInicio = proyecto.FechaInicio;
        FechaFin = proyecto.FechaFin;
        IdArea = proyecto.IdArea;
        IdContrato = proyecto.IdContrato;
        RegistrarActualizacion(actualizadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
