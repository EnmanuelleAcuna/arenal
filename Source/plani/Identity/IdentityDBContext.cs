using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace plani.Identity;

public class IdentityDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
    IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Usuarios");
            b.HasQueryFilter(e => !e.IsDeleted);
        });
        
        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
            b.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
