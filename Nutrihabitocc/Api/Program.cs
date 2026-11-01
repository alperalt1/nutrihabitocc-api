using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nutrihabitocc.Api.Services;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;
using Nutrihabitocc.Domain.Services;
using Nutrihabitocc.Infraestructure.Persistence;
using Nutrihabitocc.Infraestructure.Persistence.Repositories;
using Nutrihabitocc.Infraestructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("BearerAuth", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Introduce tu token JWT",
        });
        return Task.CompletedTask;
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("La configuración Jwt:Key es obligatoria.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Email
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler,
    ApiAuthorizationMiddlewareResultHandler>();
builder.Services.AddHttpClient();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminOnly, policy =>
        policy.RequireRole(AppRoles.Administrador));

    options.AddPolicy(AuthPolicies.AdminWebOnly, policy =>
        policy.RequireRole(AppRoles.Administrador)
            .RequireClaim("platform", Platforms.Web));

    options.AddPolicy(AuthPolicies.StaffOnly, policy =>
        policy.RequireRole(AppRoles.Administrador, AppRoles.Nutricionista));

    options.AddPolicy(AuthPolicies.WebStaffOnly, policy =>
        policy.RequireRole(AppRoles.Administrador, AppRoles.Nutricionista)
            .RequireClaim("platform", Platforms.Web));

    options.AddPolicy(AuthPolicies.PatientOnly, policy =>
        policy.RequireRole(AppRoles.Paciente));

    options.AddPolicy(AuthPolicies.MobilePatientOnly, policy =>
        policy.RequireRole(AppRoles.Paciente)
            .RequireClaim("platform", Platforms.Mobile));

    options.AddPolicy(AuthPolicies.Authenticated, policy =>
        policy.RequireAuthenticatedUser());
});
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IJwtToken, JwtToken>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMailRepository, MailRepository>();
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundWorker>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlantillaRepository, PlantillaRepository>();
builder.Services.AddScoped<IPlantillaService, PlantillaService>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();
builder.Services.AddScoped<IClinicalHistoryRepository, ClinicalHistoryRepository>();
builder.Services.AddScoped<IClinicalHistoryService, ClinicalHistoryService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddSingleton<IS3StorageService, S3StorageService>();
builder.Services.AddScoped<INutritionPlanRepository, NutritionPlanRepository>();
builder.Services.AddScoped<INutritionPlanService, NutritionPlanService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var configuration = services.GetRequiredService<IConfiguration>();
        await DbInitializer.SeedAsync(context, passwordHasher, configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar las migraciones o el Seeding de datos.");
    }
}


app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.MapOpenApi();
    app.MapScalarApiReference("/documentacion", 
        options => options
            .AddPreferredSecuritySchemes("BearerAuth")
            .AddHttpAuthentication("BearerAuth", auth =>
            {
                auth.Token = builder.Configuration["Jwt:DefaultToken"];
            })
    );
}

app.UseCors("WebApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();