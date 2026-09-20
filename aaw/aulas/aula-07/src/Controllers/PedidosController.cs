using ApiVazada.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;

    public PedidosController(AppDbContext db) => _db = db;

    private int UsuarioLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var pedido = _db.Pedidos.Find(id);
        if (pedido is null) return NotFound();
        if (pedido.UsuarioId != UsuarioLogadoId) return Forbid();
        return Ok(pedido);
    }

    [Authorize]
    [HttpGet("usuario/{usuarioId}")]
    public IActionResult GetByUsuario(int usuarioId)
    {
        var pedidos = _db.Pedidos.Where(p => p.UsuarioId == usuarioId).ToList();
        return Ok(_db.Pedidos.Where(p => p.UsuarioId == usuarioId).ToList());
    }
}
