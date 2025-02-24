using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using plani.Models;

namespace plani.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string nombre) : base(nombre)
    {
    }

    public ApplicationRole(string id, string nombre, string descripcion) : base(nombre)
    {
        Id = id;
        Name = nombre;
        Description = descripcion;
    }

    public string Description { get; set; }

    // [NotMapped]
    // public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    // [NotMapped] public override string ConcurrencyStamp { get; set; }

    // [NotMapped] public override string NormalizedName { get; set; }
    
    public DateTime DateCreated { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime? DateDeleted { get; set; }

    public void RegristrarCreacion(string creadoPor, DateTime creadoEl)
    {
        CreatedBy = creadoPor;
        DateCreated = creadoEl;
    }

    public void RegistrarActualizacion(string actualizadoPor, DateTime actualizadoEl)
    {
        UpdatedBy = actualizadoPor;
        DateUpdated = actualizadoEl;
    }

    private void RegistrarEliminacion(string eliminadoPor, DateTime eliminadoEl)
    {
        DeletedBy = eliminadoPor;
        DateDeleted = eliminadoEl;
    }
    
    public void Eliminar(string eliminadoPor)
    {
        if (IsDeleted)
            throw new InvalidOperationException(Utils.MensajeErrorObjetoYaEliminado);
        
        IsDeleted = true;
        RegistrarEliminacion(eliminadoPor, DateTime.UtcNow);
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
