using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Domain;

namespace plani.Models.Data;

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<ApplicationUser> Usuarios { get; set; }
    public virtual DbSet<ApplicationRole> Roles { get; set; }
    public virtual DbSet<TipoCliente> TiposCliente { get; set; }
    public virtual DbSet<Cliente> Clientes { get; set; }
    public virtual DbSet<Contrato> Contratos { get; set; }
    public virtual DbSet<Proyecto> Proyectos { get; set; }
    public virtual DbSet<Area> Areas { get; set; }
    public virtual DbSet<Modalidad> Modalidades { get; set; }
    public virtual DbSet<Servicio> Servicios { get; set; }
    public virtual DbSet<Asignacion> Asignaciones { get; set; }
    public virtual DbSet<Sesion> Sesiones { get; set; }

    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("DefaultConnection");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(b => { b.ToTable("Usuarios"); });
        modelBuilder.Entity<ApplicationUser>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApplicationRole>(b => { b.ToTable("Roles"); });

        modelBuilder.Entity<TipoCliente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Cliente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contrato>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Proyecto>().HasQueryFilter(e => !e.IsDeleted);

        // Relación Proyecto -> Responsable (ApplicationUser)
        modelBuilder.Entity<Proyecto>()
            .HasOne(p => p.Responsable)
            .WithMany(u => u.ProyectosACargo)
            .HasForeignKey(p => p.IdResponsable)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Area>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Modalidad>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Servicio>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Asignacion>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Sesion>().HasQueryFilter(e => !e.IsDeleted);
    }
}
