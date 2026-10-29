using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsuariosApi.Models;

namespace UsuariosApi.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) Create(Usuario usuario);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) Create(Usuario usuario)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key.");
        var issuer = _configuration["Jwt:Issuer"] ?? "UsuariosApi";
        var audience = _configuration["Jwt:Audience"] ?? "UsuariosWeb";
        var minutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 480;
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
            ],
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
