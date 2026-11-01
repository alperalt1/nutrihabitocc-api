using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Infraestructure.Persistence;

public class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
        
        await EnsureRoleAsync(context, AppRoles.Paciente, "Usuario que tendrá acceso limitado");
        await EnsureRoleAsync(context, AppRoles.Nutricionista, "Usuario profesional");
        await EnsureRoleAsync(context, AppRoles.Administrador, "Usuario con acceso total al sistema");

        await context.SaveChangesAsync();

        await SeedBootstrapAdminAsync(context, passwordHasher, configuration);
    }

    private static async Task EnsureRoleAsync(AppDbContext context, string name, string description)
    {
        var role = await context.Set<Role>().FirstOrDefaultAsync(r => r.Nombre == name);
        if (role is null)
        {
            await context.Set<Role>().AddAsync(new Role
            {
                Nombre = name,
                Descripcion = description
            });
            return;
        }

        if (!role.IsActive)
        {
            role.IsActive = true;
            role.DeletedBy = null;
            role.DeletedOn = null;
        }

        if (string.IsNullOrWhiteSpace(role.Descripcion))
        {
            role.Descripcion = description;
        }
    }

    private static async Task SeedBootstrapAdminAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        var adminRole = await context.Set<Role>()
            .FirstAsync(role => role.Nombre == AppRoles.Administrador);

        var hasAdmin = await context.Set<UserRole>()
            .AnyAsync(userRole => userRole.RoleId == adminRole.Id && userRole.IsActive);

        if (hasAdmin)
        {
            return;
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existingUser = await context.Users
            .Include(user => user.UserRoles)
            .FirstOrDefaultAsync(user => user.Email == email);

        if (existingUser is null)
        {
            existingUser = new User
            {
                Nombres = configuration["BootstrapAdmin:Nombres"] ?? "Administrador",
                Apellidos = configuration["BootstrapAdmin:Apellidos"] ?? "Sistema",
                Email = email,
                Username = email,
                Phone = configuration["BootstrapAdmin:Phone"] ?? string.Empty,
                Password = passwordHasher.HashPassword(password),
                IsActive = true,
                CreatedBy = "Bootstrap"
            };

            existingUser.UserRoles.Add(new UserRole
            {
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedBy = "Bootstrap"
            });

            context.Users.Add(existingUser);
        }
        else
        {
            existingUser.IsActive = true;
            var existingRole = existingUser.UserRoles.FirstOrDefault(userRole => userRole.RoleId == adminRole.Id);

            if (existingRole is null)
            {
                existingUser.UserRoles.Add(new UserRole
                {
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedBy = "Bootstrap"
                });
            }
            else if (!existingRole.IsActive)
            {
                existingRole.IsActive = true;
                existingRole.DeletedBy = null;
                existingRole.DeletedOn = null;
            }
        }

        await context.SaveChangesAsync();
    }
}
