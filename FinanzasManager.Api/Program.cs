using FinanzasManager.Api.Handlers;
using FinanzasManager.Application;
using FinanzasManager.Application.Common;
using FinanzasManager.Infrastructure;
using FinanzasManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS para el frontend Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4201") // URL por defecto de Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var secretKey = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Una policy por cada permiso declarado en Permisos. Se derivan de las constantes
// y no de la base, para que la API pueda arrancar aunque la base no este disponible.
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in Permisos.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim(Permisos.Claim, permiso));
    }

    options.AddPolicy(Permisos.PolicyAdministrador, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(Permisos.NombreRolAdministrador));
});

var app = builder.Build();

// Siembra los permisos y roles base. Es idempotente.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.UseHttpsRedirection();

// Orden correcto del pipeline:
app.UseExceptionHandler();
app.UseCors("PermitirAngular");
app.UseAuthentication();  // 1. Primero verifica quién es el usuario
app.UseAuthorization();   // 2. Luego verifica qué permisos tiene

app.MapControllers();

app.Run();