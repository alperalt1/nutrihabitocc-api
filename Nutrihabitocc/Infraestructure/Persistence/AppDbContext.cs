using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Entities;

namespace Nutrihabitocc.Infraestructure.Persistence;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserOtp> UserOtps { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; } = null!;
    public DbSet<PlantillaFormulario> PlantillasFormulario { get; set; } = null!;
    public DbSet<ClinicalHistory> ClinicalHistories { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<PrecioHistorial> PreciosHistorial { get; set; } = null!;
    public DbSet<CorridaScraping> CorridasScraping { get; set; } = null!;
    public DbSet<Receta> Recetas { get; set; } = null!;
    public DbSet<RecetaIngrediente> RecetaIngredientes { get; set; } = null!;
    public DbSet<MenuPlantilla> MenuPlantillas { get; set; } = null!;
    public DbSet<MenuPlantillaDia> MenuPlantillaDias { get; set; } = null!;
    public DbSet<MenuPlantillaComida> MenuPlantillaComidas { get; set; } = null!;
    public DbSet<MenuMensual> MenusMensuales { get; set; } = null!;
    public DbSet<MenuDia> MenuDias { get; set; } = null!;
    public DbSet<MenuComida> MenuComidas { get; set; } = null!;
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(rt => rt.Platform).HasMaxLength(20).IsRequired();
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.HasKey(e => e.Id);
        
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsRequired();
            entity.HasOne(otp => otp.User)
                .WithMany(u => u.UserOtps)
                .HasForeignKey(otp => otp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(r => r.Descripcion).HasMaxLength(250);
        });
        
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.UserId)
                .IsUnique();
            entity.HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ClinicalHistory>(entity =>
        {
            entity.HasKey(ch => ch.Id);
            entity.Property(ch => ch.ContentJson)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(ch => ch.FormularioSnapshotJson)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.HasOne(ch => ch.User)
                .WithMany(u => u.ClinicalHistory)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ch => ch.Workspace)
                .WithMany(w => w.ClinicalHistory)
                .HasForeignKey(ch => ch.WorkspaceId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(ch => ch.PlantillaFormulario)
                .WithMany()
                .HasForeignKey(ch => ch.PlantillaFormularioId)
                .OnDelete(DeleteBehavior.SetNull);
            
        });
        
        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.HasKey(wm => wm.Id);
            entity.Property(wm => wm.MemberRole).HasMaxLength(50).IsRequired();
            entity.HasIndex(wm => new { wm.UserId, wm.WorkspaceId })
                .HasDatabaseName("IX_WorkspaceMembers_UserId_WorkspaceId")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasIndex(wm => new { wm.WorkspaceId, wm.Id })
                .HasDatabaseName("IX_WorkspaceMembers_WorkspaceId");
            entity.HasIndex(wm => wm.WorkspaceId)
                .HasDatabaseName("IX_WorkspaceMembers_WorkspaceId_Paciente")
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"MemberRole\" = 'Paciente'");

            entity.HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMembers)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wm => wm.Workspace)
                .WithMany(w => w.WorkspaceMembers)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).HasMaxLength(150).IsRequired();
            entity.Property(w => w.Description).HasMaxLength(500);
        });
        
        modelBuilder.Entity<PlantillaFormulario>(entity =>
        {
            entity.HasKey(pf => pf.Id);
            entity.Property(pf => pf.Name).HasMaxLength(150).IsRequired();
            entity.Property(pf => pf.Estructura).HasColumnType("jsonb").IsRequired();
            entity.Property(pf => pf.Version).HasMaxLength(20).IsRequired();
        });

        // Tablas creadas y administradas por el servicio Python de scraping.
        // Se excluyen de las migraciones de EF Core para que no las cree ni las altere.
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("producto", table => table.ExcludeFromMigrations());
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(p => p.Tienda).HasColumnName("tienda").HasMaxLength(30).IsRequired();
            entity.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(120).IsRequired();
            entity.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(300).IsRequired();
            entity.Property(p => p.Marca).HasColumnName("marca").HasMaxLength(120);
            entity.Property(p => p.Categoria).HasColumnName("categoria").HasMaxLength(120);
            entity.Property(p => p.Subcategoria).HasColumnName("subcategoria").HasMaxLength(120);
            entity.Property(p => p.Presentacion).HasColumnName("presentacion").HasMaxLength(60);
            entity.Property(p => p.Precio).HasColumnName("precio").HasPrecision(10, 2);
            entity.Property(p => p.PrecioRegular).HasColumnName("precio_regular").HasPrecision(10, 2);
            entity.Property(p => p.PrecioUnidad).HasColumnName("precio_unidad").HasMaxLength(60);
            entity.Property(p => p.Url).HasColumnName("url");
            entity.Property(p => p.ImagenUrl).HasColumnName("imagen_url");
            entity.Property(p => p.ImagenKey).HasColumnName("imagen_key");
            entity.Property(p => p.Descripcion).HasColumnName("descripcion");
            entity.Property(p => p.Disponible).HasColumnName("disponible");
            entity.Property(p => p.Revisar).HasColumnName("revisar");
            entity.Property(p => p.PrimeraVezVisto).HasColumnName("primera_vez_visto");
            entity.Property(p => p.UltimaVezVisto).HasColumnName("ultima_vez_visto");
            entity.Property(p => p.UltimaActualizacion).HasColumnName("ultima_actualizacion");

            entity.HasMany(p => p.Historial)
                .WithOne(h => h.Producto)
                .HasForeignKey(h => h.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrecioHistorial>(entity =>
        {
            entity.ToTable("precio_historial", table => table.ExcludeFromMigrations());
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(h => h.ProductoId).HasColumnName("producto_id");
            entity.Property(h => h.Precio).HasColumnName("precio").HasPrecision(10, 2);
            entity.Property(h => h.PrecioRegular).HasColumnName("precio_regular").HasPrecision(10, 2);
            entity.Property(h => h.Fecha).HasColumnName("fecha");
        });

        modelBuilder.Entity<CorridaScraping>(entity =>
        {
            entity.ToTable("corrida_scraping", table => table.ExcludeFromMigrations());
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(c => c.Tienda).HasColumnName("tienda").HasMaxLength(30).IsRequired();
            entity.Property(c => c.Inicio).HasColumnName("inicio");
            entity.Property(c => c.Fin).HasColumnName("fin");
            entity.Property(c => c.Estado).HasColumnName("estado").HasMaxLength(20);
            entity.Property(c => c.Vistos).HasColumnName("vistos");
            entity.Property(c => c.Nuevos).HasColumnName("nuevos");
            entity.Property(c => c.Actualizados).HasColumnName("actualizados");
            entity.Property(c => c.NoDisponibles).HasColumnName("no_disponibles");
            entity.Property(c => c.Errores).HasColumnName("errores");
            entity.Property(c => c.Detalle).HasColumnName("detalle");
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.ToTable("receta");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Descripcion).HasMaxLength(500);
            entity.Property(r => r.Instrucciones).HasColumnType("text");
            entity.Property(r => r.PorcionesBase).HasDefaultValue(1);
            entity.HasOne(r => r.Workspace)
                .WithMany()
                .HasForeignKey(r => r.WorkspaceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(r => r.Ingredientes)
                .WithOne(i => i.Receta)
                .HasForeignKey(i => i.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecetaIngrediente>(entity =>
        {
            entity.ToTable("receta_ingrediente");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Cantidad).HasPrecision(10, 2);
            entity.Property(i => i.Unidad).HasMaxLength(30).IsRequired();
            entity.Property(i => i.Notas).HasMaxLength(500);
            entity.HasIndex(i => new { i.RecetaId, i.ProductoId })
                .HasDatabaseName("IX_RecetaIngrediente_Receta_Producto")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasOne(i => i.Producto)
                .WithMany()
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuPlantilla>(entity =>
        {
            entity.ToTable("menu_plantilla");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Descripcion).HasMaxLength(500);
            entity.HasOne(p => p.Workspace)
                .WithMany()
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(p => p.Dias)
                .WithOne(d => d.MenuPlantilla)
                .HasForeignKey(d => d.MenuPlantillaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuPlantillaDia>(entity =>
        {
            entity.ToTable("menu_plantilla_dia");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Observaciones).HasMaxLength(500);
            entity.HasIndex(d => new { d.MenuPlantillaId, d.Dia })
                .HasDatabaseName("IX_MenuPlantillaDia_Plantilla_Dia")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasMany(d => d.Comidas)
                .WithOne(c => c.MenuPlantillaDia)
                .HasForeignKey(c => c.MenuPlantillaDiaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuPlantillaComida>(entity =>
        {
            entity.ToTable("menu_plantilla_comida");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.TipoComida).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Porciones).HasPrecision(5, 2);
            entity.Property(c => c.Observaciones).HasMaxLength(500);
            entity.HasIndex(c => new { c.MenuPlantillaDiaId, c.Orden })
                .HasDatabaseName("IX_MenuPlantillaComida_Dia_Orden")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasOne(c => c.Receta)
                .WithMany()
                .HasForeignKey(c => c.RecetaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MenuMensual>(entity =>
        {
            entity.ToTable("menu_mensual");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Estado).HasMaxLength(20).IsRequired();
            entity.Property(m => m.Observaciones).HasMaxLength(1000);
            entity.HasIndex(m => new { m.PacienteUserId, m.WorkspaceId, m.Anio, m.Mes })
                .HasDatabaseName("IX_MenuMensual_Paciente_Workspace_Anio_Mes")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasOne(m => m.Paciente)
                .WithMany()
                .HasForeignKey(m => m.PacienteUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Workspace)
                .WithMany()
                .HasForeignKey(m => m.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(m => m.Dias)
                .WithOne(d => d.MenuMensual)
                .HasForeignKey(d => d.MenuMensualId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuDia>(entity =>
        {
            entity.ToTable("menu_dia");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Observaciones).HasMaxLength(500);
            entity.HasIndex(d => new { d.MenuMensualId, d.Fecha })
                .HasDatabaseName("IX_MenuDia_Menu_Fecha")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasMany(d => d.Comidas)
                .WithOne(c => c.MenuDia)
                .HasForeignKey(c => c.MenuDiaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuComida>(entity =>
        {
            entity.ToTable("menu_comida");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.TipoComida).HasMaxLength(50).IsRequired();
            entity.Property(c => c.RecetaSnapshotJson).HasColumnType("jsonb").IsRequired();
            entity.Property(c => c.Porciones).HasPrecision(5, 2);
            entity.Property(c => c.Observaciones).HasMaxLength(500);
            entity.HasIndex(c => new { c.MenuDiaId, c.Orden })
                .HasDatabaseName("IX_MenuComida_Dia_Orden")
                .IsUnique()
                .HasFilter("\"IsActive\" = true");
            entity.HasOne(c => c.Receta)
                .WithMany()
                .HasForeignKey(c => c.RecetaId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Auditory>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.UtcNow;
                entry.Entity.IsActive = true;

                if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = "System";
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = DateTime.UtcNow;
                if (string.IsNullOrEmpty(entry.Entity.ModifiedBy))
                {
                    entry.Entity.ModifiedBy = "System";
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsActive = false;
                entry.Entity.DeletedOn = DateTime.UtcNow;
                if (string.IsNullOrEmpty(entry.Entity.DeletedBy))
                {
                    entry.Entity.DeletedBy = "System";
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}