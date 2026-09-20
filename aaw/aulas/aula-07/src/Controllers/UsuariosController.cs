using ApiVazada.Data;
using ApiVazada.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is null) return NotFound();
        return Ok(new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
    }

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Atualizar(int id, [FromBody] Usuario dados)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is null) return NotFound();

        usuario.Nome = dados.Nome;
        usuario.Email = dados.Email;

        _db.SaveChanges();
        return Ok(new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
    }
}
