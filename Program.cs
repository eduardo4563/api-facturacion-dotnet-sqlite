using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


// ======================================================
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// ======================================================
// REPOSITORIES
// ======================================================

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();


// ======================================================
// SERVICES
// ======================================================

builder.Services.AddScoped<ClienteService>();

builder.Services.AddScoped<ProductoService>();

builder.Services.AddScoped<FacturaService>();

builder.Services.AddScoped<AuthService>();


// ======================================================
// PASSWORD HASHER
// ======================================================

builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>
>();


// ======================================================
// JWT
// ======================================================

// Primero intenta obtener la clave desde la variable
// de entorno JWT_SECRET.
//
// Si no existe, intenta leer Jwt:Key desde appsettings.json.

var jwtKey =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Key"];


if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT_SECRET no está configurado."
    );
}


var key = Encoding.UTF8.GetBytes(jwtKey);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    options.SaveToken = true;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,

            ValidateAudience = false,

            ValidateIssuerSigningKey = true,

            ValidateLifetime = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key),

            ClockSkew = TimeSpan.Zero
        };
});


// ======================================================
// CONTROLLERS
// ======================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// ======================================================
// SWAGGER
// ======================================================

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "API REST de Facturación",
            Version = "v1",

            Description =
                "API REST desarrollada con ASP.NET Core 8, " +
                "Entity Framework Core, SQLite y JWT."
        }
    );


    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "Ingresa el token JWT. Ejemplo: Bearer eyJhbGciOi...",

            Name = "Authorization",

            In = ParameterLocation.Header,

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT"
        }
    );


    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        }
    );
});


// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();


// ======================================================
// MIDDLEWARE
// ======================================================

app.UseMiddleware<ExceptionMiddleware>();


// ======================================================
// DATABASE CREATION + ADMIN USER
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();


    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<
                IPasswordHasher<Usuario>
            >();


    // Crear base de datos si no existe
    db.Database.EnsureCreated();


    // Crear administrador solamente si no existe
    if (!db.Usuarios.Any(x => x.Username == "admin"))
    {
        var admin = new Usuario
        {
            Username = "admin",

            Rol = "Admin"
        };


        // La contraseña NO se guarda como texto.
        // Se guarda únicamente el hash.

        admin.PasswordHash =
            passwordHasher.HashPassword(
                admin,
                "123456"
            );


        db.Usuarios.Add(admin);

        db.SaveChanges();
    }
}


// ======================================================
// SWAGGER
// ======================================================

app.UseSwagger();

app.UseSwaggerUI();


// ======================================================
// CORS
// ======================================================

app.UseCors("AllowFrontend");


// ======================================================
// AUTHENTICATION / AUTHORIZATION
// ======================================================

app.UseAuthentication();

app.UseAuthorization();


// ======================================================
// CONTROLLERS
// ======================================================

app.MapControllers();


// ======================================================
// START
// ======================================================

app.Run();
