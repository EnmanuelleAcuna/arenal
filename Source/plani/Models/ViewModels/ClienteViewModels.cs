using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class IndexClientesViewModel
{
    [DisplayName("Filtrar por")]
    public string PalabraClave { get; set; }

    public IEnumerable<Cliente> Clientes { get; set; }
}

public class DetalleClienteViewModel
{
    public DetalleClienteViewModel(Cliente cliente)
    {
        Id = cliente.Id;
        Identificacion = cliente.Identificacion;
        Nombre = cliente.Nombre;
        Direccion = cliente.Direccion;
        Descripcion = cliente.Descripcion;
        TipoCliente = cliente.TipoCliente;
        Contratos = cliente.Contratos;
    }

    public Guid Id { get; set; }
    public string Identificacion { get; set; }
    public string Nombre { get; set; }
    public string Direccion { get; set; }
    public string Descripcion { get; set; }
    public TipoCliente TipoCliente { get; set; }
    public IEnumerable<Contrato> Contratos { get; set; }
}

public class AgregarClienteViewModel
{
    [Required(ErrorMessage = "La identificación es requerida.")]
    [StringLength(50, ErrorMessage = "La identificación debe tener máximo 50 caracteres.")]
    public string Identificacion { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(500, ErrorMessage = "La dirección debe tener máximo 500 caracteres.")]
    public string Direccion { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    public Guid IdTipoCliente { get; set; }
}

public class EditarClienteViewModel
{
    public EditarClienteViewModel() { }

    public EditarClienteViewModel(Cliente cliente)
    {
        Id = cliente.Id;
        Identificacion = cliente.Identificacion;
        Nombre = cliente.Nombre;
        Direccion = cliente.Direccion;
        Descripcion = cliente.Descripcion;
        IdTipoCliente = cliente.IdTipoCliente;
    }

    public Guid Id { get; set; }

    [Required(ErrorMessage = "La identificación es requerida.")]
    [StringLength(50, ErrorMessage = "La identificación debe tener máximo 50 caracteres.")]
    public string Identificacion { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(500, ErrorMessage = "La dirección debe tener máximo 500 caracteres.")]
    public string Direccion { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    public Guid IdTipoCliente { get; set; }
}

public class EliminarClienteViewModel
{
    public EliminarClienteViewModel() { }

    public EliminarClienteViewModel(Cliente cliente)
    {
        Id = cliente.Id;
        Nombre = cliente.Nombre;
        Identificacion = cliente.Identificacion;
    }

    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Identificacion { get; set; }
}
