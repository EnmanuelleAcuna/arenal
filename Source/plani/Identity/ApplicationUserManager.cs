using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace plani.Identity;

public class ApplicationUserManager : UserManager<ApplicationUser> {
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _store;

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
        : base(store: store,
            optionsAccessor: optionsAccessor,
            passwordHasher: passwordHasher,
            userValidators: userValidators,
            passwordValidators: passwordValidators,
            keyNormalizer: keyNormalizer,
            errors: errors,
            services: services,
            logger: logger) {
        _store = store;
        _roleManager = roleManager;
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user) {
        ApplicationUser userRecord = await FindByIdAsync(userId: user.Id);

        if (userRecord == null) {
            return IdentityResult.Failed(new IdentityError
                { Code = "UserNotFound", Description = $"No user was found with the id {user.Id}" });
        }

        userRecord.UpdatedBy = user.UpdatedBy;
        userRecord.DateUpdated = user.DateUpdated;
        userRecord.DeletedBy = user.DeletedBy;
        userRecord.DateDeleted = user.DateDeleted;
        userRecord.IsDeleted = user.IsDeleted;

        IdentityResult result = await base.UpdateAsync(user: userRecord);

        return result;
    }

    public async Task<IdentityResult> UpdateLastSession(ApplicationUser user) {
        // user.LastSession = DateTime.UtcNow;
        IdentityResult result = await _store.UpdateAsync(user: user, cancellationToken: CancellationToken);
        return result;
    }

    public async Task<IdentityResult> UpdatePersonalInformation(ApplicationUser user) {
        ApplicationUser userRecord = await FindByIdAsync(userId: user.Id);

        if (userRecord == null) {
            throw new KeyNotFoundException($"No user was found with the id {user.Id}");
        }

        userRecord.RegistrarActualizacion(actualizadoPor: user.UpdatedBy, actualizadoEl: user.DateUpdated);
        userRecord.SetNewPersonalInformation(name: user.Name, firstLastName: user.FirstLastName, secondLastName: user.SecondLastName,
            identification: user.IdentificationNumber);

        IdentityResult result = await UpdateAsync(user: userRecord);
        return result;
    }

    public async Task<IdentityResult> ActualizarRolesUsuario(ApplicationUser user, IEnumerable<string> roles) {
        ApplicationUser userRecord = await FindByIdAsync(userId: user.Id);

        if (userRecord == null) {
            throw new KeyNotFoundException($"No user was found with the id {user.Id}");
        }

        IList<string> actualRoles = await GetRolesAsync(user: userRecord);
        IdentityResult rolesUnassigned = await RemoveFromRolesAsync(user: userRecord, roles: actualRoles);
        if (!rolesUnassigned.Succeeded) {
            return rolesUnassigned;
        }

        IdentityResult rolesAssigned = await AddToRolesAsync(user: userRecord, roles: roles);

        return rolesAssigned;
    }

    public async Task<IList<ApplicationRole>> ObtenerRolesUsuario(ApplicationUser usuario) {
        IList<string> nombresRolesUsuario = await GetRolesAsync(user: usuario);

        IList<ApplicationRole> rolesUsuario = new List<ApplicationRole>();
        foreach (string nombreRol in nombresRolesUsuario) {
            ApplicationRole rol = await _roleManager.FindByNameAsync(roleName: nombreRol);

            if (rol != null) {
                rolesUsuario.Add(item: rol);
            }
        }

        return rolesUsuario;
    }
}