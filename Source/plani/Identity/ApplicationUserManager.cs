using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace plani.Identity;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly IUserStore<ApplicationUser> _store;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationUserManager(IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger,
        RoleManager<ApplicationRole> roleManager)
        : base(store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
        _store = store;
        _roleManager = roleManager;
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        var userRecord = await FindByIdAsync(user.Id);

        if (userRecord == null)
            return IdentityResult.Failed(new IdentityError
                { Code = "UserNotFound", Description = $"No user was found with the id {user.Id}" });

        userRecord.UpdatedBy = user.UpdatedBy;
        userRecord.DateUpdated = user.DateUpdated;
        userRecord.DeletedBy = user.DeletedBy;
        userRecord.DateDeleted = user.DateDeleted;
        userRecord.IsDeleted = user.IsDeleted;

        IdentityResult result = await base.UpdateAsync(userRecord);

        return result;
    }

    public async Task<IdentityResult> UpdateLastSession(ApplicationUser user)
    {
        // user.LastSession = DateTime.Now;
        IdentityResult result = await _store.UpdateAsync(user, CancellationToken);
        return result;
    }

    public async Task<IdentityResult> UpdatePersonalInformation(ApplicationUser user)
    {
        var userRecord = await FindByIdAsync(user.Id);

        if (userRecord == null)
            throw new KeyNotFoundException($"No user was found with the id {user.Id}");

        userRecord.RegistrarActualizacion(user.UpdatedBy, user.DateUpdated);
        userRecord.SetNewPersonalInformation(user.Name, user.FirstLastName, user.SecondLastName,
            user.IdentificationNumber);

        IdentityResult result = await UpdateAsync(userRecord);
        return result;
    }

    public async Task<IdentityResult> ActualizarRolesUsuario(ApplicationUser user, IEnumerable<string> roles)
    {
        var userRecord = await FindByIdAsync(user.Id);

        if (userRecord == null)
            throw new KeyNotFoundException($"No user was found with the id {user.Id}");

        IList<string> actualRoles = await GetRolesAsync(userRecord);
        IdentityResult rolesUnassigned = await RemoveFromRolesAsync(userRecord, actualRoles);
        if (!rolesUnassigned.Succeeded) return rolesUnassigned;
        IdentityResult rolesAssigned = await AddToRolesAsync(userRecord, roles);

        return rolesAssigned;
    }

    public async Task<IList<ApplicationRole>> ObtenerRolesUsuario(ApplicationUser usuario)
    {
        IList<string> nombresRolesUsuario = await GetRolesAsync(usuario);

        IList<ApplicationRole> rolesUsuario = new List<ApplicationRole>();
        foreach (string nombreRol in nombresRolesUsuario)
        {
            ApplicationRole rol = await _roleManager.FindByNameAsync(nombreRol);

            if (rol != null)
                rolesUsuario.Add(rol);
        }

        return rolesUsuario;
    }
}
