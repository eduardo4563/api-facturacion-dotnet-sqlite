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

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

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

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// ======================================================
// JWT
// ======================================================

var jwtKey =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT_SECRET no esta configurado. Define la variable de entorno antes de iniciar la API."
    );
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT_SECRET debe tener al menos 32 caracteres."
    );
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ======================================================
// CONTROLLERS + SWAGGER
// ======================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API REST de Facturacion",
        Version = "v1",
        Description =
            "API REST de gestion de facturacion desarrollada con ASP.NET Core 8, " +
            "Entity Framework Core, SQLite y autenticacion JWT. Proyecto demostrativo; " +
            "no realiza integracion fiscal con SUNAT."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa el token JWT obtenido en /api/auth/login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();

// ======================================================
// MANEJO GLOBAL DE ERRORES
// ======================================================

app.UseMiddleware<ExceptionMiddleware>();

// ======================================================
// DATABASE + USUARIO ADMIN
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<Usuario>>();

    db.Database.EnsureCreated();

    var adminUsername =
        Environment.GetEnvironmentVariable("ADMIN_USERNAME")
        ?? builder.Configuration["Admin:Username"]
        ?? "admin";

    var adminPassword =
        Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "ADMIN_PASSWORD no esta configurado. Define la variable de entorno antes de iniciar la API."
        );
    }

    if (adminPassword.Length < 8)
    {
        throw new InvalidOperationException(
            "ADMIN_PASSWORD debe tener al menos 8 caracteres."
        );
    }

    var admin = db.Usuarios.FirstOrDefault(x => x.Username == adminUsername);

    if (admin == null)
    {
        admin = new Usuario
        {
            Username = adminUsername,
            Rol = "Admin"
        };

        admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
        db.Usuarios.Add(admin);
    }
    else
    {
        // La variable ADMIN_PASSWORD es la fuente de verdad del usuario demo.
        // Si cambia, se actualiza el hash al reiniciar la API.
        admin.Rol = "Admin";
        admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
    }

    db.SaveChanges();
}

// ======================================================
// PIPELINE
// ======================================================

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
