using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using plani.Models;

namespace plani.Identity;

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

    public ApplicationUser(string id, string correo, string nombre, string primerApellido,
        string segundoApellido, string identificacion, bool activo)
    {
        Id = id;
        Email = correo.ToLower();
        UserName = correo.ToLower();
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

    public void RegistrarActualizacion(string actualizadoPor, DateTime? actualizadoEl)
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
