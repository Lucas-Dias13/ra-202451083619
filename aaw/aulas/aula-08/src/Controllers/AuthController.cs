using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var usuario = BibliotecaStore.Autenticar(request.Email, request.Senha);
        if (usuario is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var token = GerarToken(usuario);
        return Ok(new { token, nome = usuario.Nome, role = usuario.Role });
    }

    private string GerarToken(Usuario usuario)
    {
        var chave = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada em appsettings.json.");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var minutos = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(minutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("login-expirado")]
    public IActionResult LoginExpirado([FromBody] LoginRequest request)
    {
        var usuario = BibliotecaStore.Autenticar(request.Email, request.Senha);
        if (usuario is null) return Unauthorized(new { message = "Credenciais inválidas." });

        var chave = _config["Jwt:Key"]!;
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [new Claim(ClaimTypes.Name, usuario.Nome), new Claim(ClaimTypes.Role, usuario.Role)],
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: credenciais);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), aviso = "Este token já está expirado." });
    }
}
