using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace arenal.Identity;

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

    [NotMapped] public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    [NotMapped] public override string ConcurrencyStamp { get; set; }

    [NotMapped] public override string NormalizedName { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this);
}
