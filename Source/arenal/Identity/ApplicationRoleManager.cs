using Microsoft.AspNetCore.Identity;

namespace arenal.Identity;

public class ApplicationRoleManager<TRole> : RoleManager<ApplicationRole>
{
    private readonly IRoleStore<ApplicationRole> _store;

    public ApplicationRoleManager(IRoleStore<ApplicationRole> store,
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<RoleManager<ApplicationRole>> logger)
        : base(store,
            roleValidators,
            keyNormalizer,
            errors,
            logger)
    {
        _store = store;
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationRole role)
    {
        ApplicationRole roleRecord = await FindByIdAsync(role.Id);

        if (roleRecord == null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = $"No role was found with the id {role.Id}" });
        }

        roleRecord.Name = role.Name;
        roleRecord.Description = role.Description;

        IdentityResult result = await base.UpdateAsync(roleRecord);

        return result;
    }
}
