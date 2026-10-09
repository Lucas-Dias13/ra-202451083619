using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly BibliotecaStore _store;

    public LivrosController(BibliotecaStore store) => _store = store;

    [HttpGet]
    public ActionResult<IEnumerable<Livro>> Listar() => Ok(_store.Listar());

    [HttpGet("{id}")]
    public ActionResult<Livro> BuscarPorId(int id)
    {
        var livro = _store.BuscarPorId(id);
        return livro is null ? NotFound() : Ok(livro);
    }

    [Authorize]
    [HttpPost]
    public ActionResult<Livro> Criar([FromBody] LivroRequest dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Titulo))
            return BadRequest(new { message = "Título é obrigatório." });

        var livro = _store.Adicionar(dados);
        return CreatedAtAction(nameof(BuscarPorId), new { id = livro.Id }, livro);
    }

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Atualizar(int id, [FromBody] LivroRequest dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Titulo))
            return BadRequest(new { message = "Título é obrigatório." });

        return _store.Atualizar(id, dados) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Bibliotecario")]
    [HttpDelete("{id}")]
    public IActionResult Remover(int id) =>
        _store.Remover(id) ? NoContent() : NotFound();

    [Authorize]
    [HttpGet("/api/quem-sou-eu")]
    public IActionResult QuemSouEu()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { autenticado = false, dica = "Faça login e mande o header Authorization: Bearer <token>." });

        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new { autenticado = true, claims });
    }
}
