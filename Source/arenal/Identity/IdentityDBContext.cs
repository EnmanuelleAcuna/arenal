using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace arenal.Identity;

public class IdentityDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
	public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Usuarios
		modelBuilder.Entity<ApplicationUser>(b =>
		{
			//b.ToTable("USUARIOS"); // Remap to table with different name

			b.HasMany<ApplicationUserRole>(e => e.UserRoles) // Each User can have many entries in the UserRole join table
			 .WithOne(e => e.User)
			 .HasForeignKey(ur => ur.UserId)
			 .IsRequired();
		});

		// Roles
		modelBuilder.Entity<ApplicationRole>(b =>
		{
			//b.ToTable("ROLES"); // Remap to table with different name

			b.HasMany<ApplicationUserRole>(e => e.UserRoles) // Each Role can have many entries in the UserRole join table
			 .WithOne(e => e.Role)
			 .HasForeignKey(ur => ur.RoleId)
			 .IsRequired();
		});

		// Users roles
		modelBuilder.Entity<ApplicationUserRole>(b =>
		{
			//b.ToTable("ROLES_USUARIO"); // Remap to table with different name
		});

	}
}
