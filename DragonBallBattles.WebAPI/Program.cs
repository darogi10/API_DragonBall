using Microsoft.AspNetCore.Authentication.JwtBearer;
using DragonBallBattles.Application.Interfaces;
using DragonBallBattles.Application.Services;
using DragonBallBattles.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// INICIO: CONFIGURACIÓN DE SWAGGER/OPENAPI
// Estas líneas añaden los servicios necesarios para generar la documentación de la API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Definir el esquema de seguridad para JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT obtenido del endpoint de Login. Ejemplo: Bearer [tu_token]",
    });

    // 2. Especificar qué operaciones requieren este esquema de seguridad
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// FIN: CONFIGURACIÓN DE SWAGGER/OPENAPI

// Configurar servicios para la inyección de dependencias
builder.Services.AddControllers();

// Logging
// Especifica explícitamente tu ILogger personalizado
builder.Services.AddSingleton<DragonBallBattles.Application.Interfaces.ILogger, ConsoleLogger>();

builder.Services.AddSingleton<HttpClient>();

// Inyección de dependencias para nuestras capas de Arquitectura Limpia
builder.Services.AddScoped<IDragonBallApiClient, DragonBallApiClient>();
builder.Services.AddScoped<BattleSchedulingService>();

builder.Services.AddScoped<DragonBallBattles.Application.Interfaces.IAuthService, DragonBallBattles.Application.Services.AuthService>();

// Configuración de JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("La clave JWT (Jwt:Key) no está configurada en appsettings.json.");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configuración de Autorización
builder.Services.AddAuthorization();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "https://tu-dominio-frontend.com")
                            .AllowAnyHeader()
                            .AllowAnyMethod());
});

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Estas líneas habilitan la interfaz de usuario de Swagger.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();