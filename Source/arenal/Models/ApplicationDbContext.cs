using arenal.Models.Entities;
using arenal.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace arenal.Models;

public class ApplicationDbContext : DbContext
{
	public virtual DbSet<Provincia> Provincias { get; set; }
	public virtual DbSet<Canton> Cantones { get; set; }
	public virtual DbSet<Distrito> Distritos { get; set; }
	public virtual DbSet<Contacto> Contactos { get; set; }
	public virtual DbSet<TipoMaquina> TiposMaquina { get; set; }
	public virtual DbSet<Maquina> Maquinas { get; set; }
	public virtual DbSet<TipoEjercicio> TiposEjercicio { get; set; }
	public virtual DbSet<Ejercicio> Ejercicios { get; set; }
	public virtual DbSet<TipoMedida> TiposMedida { get; set; }
	public virtual DbSet<GrupoMuscular> GruposMusculares { get; set; }
	public virtual DbSet<Rutina> Rutinas { get; set; }
	public virtual DbSet<ApplicationUser> Usuarios { get; set; }
	// public virtual DbSet<DetalleMedidas> DetalleMedidas { get; set; }
	// public virtual DbSet<DetalleRutina> DetalleRutina { get; set; }
	// public virtual DbSet<GruposMuscularesEjercicio> GruposMuscularesEjercicio { get; set; }
	
	public ApplicationDbContext() { }

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
			optionsBuilder.UseSqlServer("DefaultConnection");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		
		modelBuilder.Entity<ApplicationUser>(b =>
		{
			b.ToTable("AspNetUsers", "dbo"); // Remap to table with different name
		});
	}
}
