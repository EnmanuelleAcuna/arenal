﻿using arenal.Models;
using arenal.Identity;
using Microsoft.EntityFrameworkCore;

namespace arenal.Data;

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<ApplicationUser> Usuarios { get; set; }
    public virtual DbSet<ApplicationRole> Roles { get; set; }
    public virtual DbSet<TipoCliente> TiposCliente { get; set; }
    public virtual DbSet<Cliente> Clientes { get; set; }
    public virtual DbSet<Contrato> Contratos { get; set; }
    public virtual DbSet<Area> Areas { get; set; }
    public virtual DbSet<Modalidad> Modalidades { get; set; }
    public virtual DbSet<Servicio> Servicios { get; set; }

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

        modelBuilder.Entity<ApplicationUser>(b => { b.ToTable("AspNetUsers", "dbo"); });
        modelBuilder.Entity<ApplicationRole>(b => { b.ToTable("AspNetRoles", "dbo"); });

        modelBuilder.Entity<TipoCliente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Cliente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contrato>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<Area>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Modalidad>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Servicio>().HasQueryFilter(e => !e.IsDeleted);
    }
}