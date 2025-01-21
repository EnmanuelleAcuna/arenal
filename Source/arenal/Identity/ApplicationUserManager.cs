using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace arenal.Identity;

public class ApplicationUserManager<TUser> : UserManager<ApplicationUser>
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

		userRecord.SetNewPersonalInformation(user.Name, user.FirstLastName, user.SecondLastName, user.IdentificationNumber);

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

	public async Task<IdentityResult> RegistrarUsuarioComoInstructor(ApplicationUser user, string filePath)
	{
		var userRecord = await FindByIdAsync(user.Id);

		if (userRecord == null)
			throw new KeyNotFoundException($"No user was found with the id {user.Id}");

		IList<string> roles = await GetRolesAsync(user);

		bool isInstructor = roles.Any(role => role.Equals("Instructor", StringComparison.OrdinalIgnoreCase));

		if (!isInstructor)
		{
			roles.Add("Instructor");
			var rolesActualizados = await ActualizarRolesUsuario(user, roles);
			if (!rolesActualizados.Succeeded) return rolesActualizados;
		}

		IdentityResult result = await UpdateAsync(userRecord);
		return result;
	}

	public async Task<IdentityResult> RegistrarUsuarioComoCliente(ApplicationUser user, string filePath)
	{
		var userRecord = await FindByIdAsync(user.Id);

		if (userRecord == null)
			throw new KeyNotFoundException($"No user was found with the id {user.Id}");

		IList<string> roles = await GetRolesAsync(user);

		bool isCliente = roles.Any(role => role.Equals("Cliente", StringComparison.OrdinalIgnoreCase));

		if (!isCliente)
		{
			roles.Add("Cliente");
			var rolesActualizados = await ActualizarRolesUsuario(user, roles);
			if (!rolesActualizados.Succeeded) return rolesActualizados;
		}

		IdentityResult result = await UpdateAsync(userRecord);
		return result;
	}

	public async Task<IList<ApplicationUser>> GetUsersNotInRoleAsync(string roleName)
	{
		if (await _roleManager.FindByNameAsync(roleName) == null)
			throw new InvalidOperationException($"Rol '{roleName}' no encontrado.");

		var usersInRole = await GetUsersInRoleAsync(roleName);
		var allUsers = await Users.ToListAsync();
		var usersNotInRole = allUsers.Except(usersInRole).ToList();
		return usersNotInRole;
	}

	public async Task<IList<ApplicationUser>> GetUsersInRoleWithDivisionTerritorialInfoAsync(string roleName)
	{
		if (await _roleManager.FindByNameAsync(roleName) == null)
			throw new InvalidOperationException($"Rol '{roleName}' no encontrado.");

		var usersInRole = await GetUsersInRoleAsync(roleName);

		return usersInRole;
	}
	
	public async Task<IList<ApplicationRole>> ObtenerRolesUsuario(ApplicationUser usuario)
	{
		IList<string> nombresRolesUsuario = await GetRolesAsync(usuario);

		IList<ApplicationRole> rolesUsuario = new List<ApplicationRole>();
		foreach (string nombreRol in nombresRolesUsuario)
		{
			ApplicationRole rol = await _roleManager.FindByNameAsync(nombreRol);
			rolesUsuario.Add(rol);
		}

		return rolesUsuario;
	}
}
