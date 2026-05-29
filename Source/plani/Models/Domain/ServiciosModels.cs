using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace plani.Models.Domain;

[Table("Areas")]
public class Area : Base {
    public Area() {
        Servicios = new List<Servicio>();
        Contratos = new List<Contrato>();
        Proyectos = new List<Proyecto>();
    }

    public Area(Guid id, string nombre, string descripcion) {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        Servicios = new List<Servicio>();
        Contratos = new List<Contrato>();
        Proyectos = new List<Proyecto>();
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

    public ICollection<Servicio> Servicios { get; set; }
    public ICollection<Contrato> Contratos { get; set; }
    public ICollection<Proyecto> Proyectos { get; set; }

    public void Actualizar(Area area, string actualizadoPor) {
        Nombre = area.Nombre;
        Descripcion = area.Descripcion;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Modalidades")]
public class Modalidad : Base {
    public Modalidad() {
        Servicios = new List<Servicio>();
    }

    public Modalidad(Guid id, string nombre, string descripcion) {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        Servicios = new List<Servicio>();
    }

    [Key] public Guid Id { get; set; }

    [StringLength(maximumLength: 250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 50 ? Descripcion.Substring(startIndex: 0, length: 50) + "..." : Descripcion;

    public ICollection<Servicio> Servicios { get; set; }

    public void Actualizar(Modalidad modalidad, string actualizadoPor) {
        Nombre = modalidad.Nombre;
        Descripcion = modalidad.Descripcion;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}

[Table("Servicios")]
public class Servicio : Base {
    public Servicio() { }

    public Servicio(Guid id, string nombre, string descripcion, Area area, Modalidad modalidad) {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        IdArea = area.Id;
        Area = area;

        IdModalidad = modalidad.Id;
        Modalidad = modalidad;
    }

    public Servicio(Guid id, string nombre, string descripcion, Guid idArea, Guid idModalidad) {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;

        IdArea = idArea;
        IdModalidad = idModalidad;
    }

    [Key] public Guid Id { get; set; }

    [StringLength(maximumLength: 250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(maximumLength: 2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [NotMapped]
    public string TruncatedDescripcion =>
        Descripcion?.Length > 20 ? Descripcion.Substring(startIndex: 0, length: 20) + "..." : Descripcion;

    [ForeignKey(nameof(Area))] public Guid IdArea { get; set; }
    public Area Area { get; set; }

    [ForeignKey(nameof(Modalidad))] public Guid IdModalidad { get; set; }
    public Modalidad Modalidad { get; set; }

    public void Actualizar(Servicio servicio, string actualizadoPor) {
        Nombre = servicio.Nombre;
        Descripcion = servicio.Descripcion;
        IdArea = servicio.IdArea;
        IdModalidad = servicio.IdModalidad;
        RegistrarActualizacion(actualizadoPor: actualizadoPor, actualizadoEl: DateTime.UtcNow);
    }

    public override string ToString() {
        return JsonSerializer.Serialize(this);
    }
}