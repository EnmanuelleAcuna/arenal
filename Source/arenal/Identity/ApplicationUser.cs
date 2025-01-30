using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using arenal.Models;
using Microsoft.AspNetCore.Identity;

namespace arenal.Identity;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser() : base()
    {
        Asignaciones = new List<Asignacion>();
        Sesiones = new List<Sesion>();
    }

    public ApplicationUser(string userName) : base(userName)
    {
        Asignaciones = new List<Asignacion>();
        Sesiones = new List<Sesion>();
    }

    public ApplicationUser(string id, string correo, string nombreUsuario, string nombre, string primerApellido,
        string segundoApellido, string identificacion, bool activo)
    {
        Id = id;
        Email = correo;
        UserName = nombreUsuario;
        Name = nombre;
        FirstLastName = primerApellido;
        SecondLastName = segundoApellido;
        IdentificationNumber = identificacion;
        Active = activo;

        Asignaciones = new List<Asignacion>();
        Sesiones = new List<Sesion>();
    }

    public string IdentificationNumber { get; private set; }
    public string Name { get; private set; }
    public string FirstLastName { get; private set; }
    public string SecondLastName { get; private set; }

    public bool? Active { get; private set; }

    [NotMapped] public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    
    #region Identity properties that does not need to be mapped in the DB

    [NotMapped] public override bool EmailConfirmed { get; set; }

    [NotMapped] public override string PhoneNumber { get; set; }

    [NotMapped] public override bool PhoneNumberConfirmed { get; set; }

    [NotMapped] public override bool TwoFactorEnabled { get; set; }

    [NotMapped] public override DateTimeOffset? LockoutEnd { get; set; }

    [NotMapped] public override bool LockoutEnabled { get; set; }

    [NotMapped] public override int AccessFailedCount { get; set; }

    [NotMapped] public override string ConcurrencyStamp { get; set; }

    [NotMapped] public override string NormalizedEmail { get; set; }

    [NotMapped] public override string NormalizedUserName { get; set; }

    #endregion

    [NotMapped]
    public string FullName
    {
        get { return $"{Name} {FirstLastName} {SecondLastName}"; }
    }
    
    public ICollection<Asignacion> Asignaciones { get; set; }
    public ICollection<Sesion> Sesiones { get; set; }
    
    public void SetNewPersonalInformation(string name, string firstLastName, string secondLastName,
        string identification)
    {
        Name = name;
        FirstLastName = firstLastName;
        SecondLastName = secondLastName;
        IdentificationNumber = identification;
    }

    public override string ToString() => JsonSerializer.Serialize(this);
}
