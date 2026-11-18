using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.Data.Seed;
using SistemaNotasColegio.Services;
using SistemaNotasColegio.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
    || string.IsNullOrWhiteSpace(jwtOptions.Audience)
    || jwtOptions.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException(
        "La configuración JWT es inválida. Configure Jwt:Issuer, Jwt:Audience y Jwt:ExpirationMinutes.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException(
        "La clave JWT no está configurada o es demasiado corta. " +
        "Configure Jwt:Key mediante User Secrets o variables de entorno con al menos 32 bytes.");
}

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.Configure<InitialSecretariaOptions>(
    builder.Configuration.GetSection(InitialSecretariaOptions.SectionName));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.Secretaria,
        policy => policy.RequireRole(SistemaNotasColegio.Models.Roles.Secretaria));

    options.AddPolicy(
        AuthorizationPolicies.Profesor,
        policy => policy.RequireRole(SistemaNotasColegio.Models.Roles.Profesor));
});

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IMateriaService, MateriaService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Notas Escolar API",
        Version = "v1",
        Description = "API para la gestión de estudiantes, materias, asignaciones y calificaciones."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Swagger agrega automáticamente el prefijo Bearer."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema de Notas Escolar API v1");
    });
}

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
