using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public string Login(LoginDto dto)
    {
        // Buscar usuario por nombre
        var user = _context.Usuarios
            .FirstOrDefault(x => x.Username == dto.Username);

        if (user == null)
        {
            throw new Exception("Credenciales incorrectas.");
        }

        // Verificar la contraseña ingresada contra el hash guardado
        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password
        );

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new Exception("Credenciales incorrectas.");
        }

        // Obtener la clave JWT
        var jwtKey =
            Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "La clave JWT no está configurada."
            );
        }

        var key = Encoding.UTF8.GetBytes(jwtKey);

        // Datos incluidos dentro del token
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddHours(3),

            Issuer = _configuration["Jwt:Issuer"],

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }
}
