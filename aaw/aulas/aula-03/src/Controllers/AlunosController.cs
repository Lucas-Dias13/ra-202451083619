using EscolaApi.Data;
using EscolaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Controllers;

// ============================================================================
//  API DA ESCOLATECH — ponto de partida da prática da Aula 03.
//  Esta API FUNCIONA, mas cada endpoint carrega um anti-padrão de design
//  (os mesmos 6 do handout). Sua missão: refatorá-la para o design correto.
//  Procure os comentários "ANTI-PADRÃO N" e consulte o roteiro no README.md.
// ============================================================================
[ApiController]
[Route ("api/v1/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext db;
    public AlunosController(AppDbContext db) { this.db = db; }

    // ANTI-PADRÃO 1 — verbo na URI e ausência de versionamento (ANTI-PADRÃO 4):
    // a rota deveria ser um substantivo versionado: GET /api/v1/alunos
    // ANTI-PADRÃO 5 — sem paginação: devolve os 120 alunos (com matrículas!)
    // de uma vez. Imagine 120 mil.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1 || size > 50)
            return Problem(
                title: "Parâmetros de paginação inválidos",
                detail: "page >= 1 e 1 <= size <= 50.",
                statusCode: StatusCodes.Status400BadRequest);

        var total = await db.Alunos.CountAsync();
        var alunos = await db.Alunos
            .OrderBy(a => a.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new { a.Id, a.Nome, a.Curso })
            .ToListAsync();

        return Ok(new
        {
            page,
            size,
            totalItens = total,
            totalPaginas = (int)Math.Ceiling(total / (double)size),
            itens = alunos,
        });
    }

    // ANTI-PADRÃO 2 — status code errado: id inexistente devolve 200 com corpo
    // nulo em vez de 404. O cliente só descobre o problema quando explode.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var aluno = await db.Alunos.Include(a => a.Matriculas)
                                   .FirstOrDefaultAsync(a => a.Id == id);
        if (aluno is null) return NotFound();

        return Ok(aluno);
    }

    // ANTI-PADRÃO 1 (de novo) — POST /deletarAluno: verbo errado NA URI e
    // método HTTP errado PARA A AÇÃO (deveria ser DELETE /api/v1/alunos/{id}).
    // ANTI-PADRÃO 2 — devolve 200 com mensagem de texto em vez de 204/404.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var aluno = await db.Alunos.FindAsync(id);
        if (aluno is null) return NotFound();

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ANTI-PADRÃO 3 — aninhamento profundo demais: 4 níveis para chegar numa
    // matrícula. A regra prática é no MÁXIMO 2 níveis de recurso:
    // /alunos/{id}/matriculas/{matriculaId}
    [HttpGet("{id}/matriculas")]
    public async Task<IActionResult> GetMatriculas(int id)
    {
        var existe = await db.Alunos.AnyAsync(a => a.Id == id);
        if (!existe) return NotFound();           // 404 + ProblemDetails, sem HTML

        var matriculas = await db.Matriculas.Where(m => m.AlunoId == id).ToListAsync();
        return Ok(matriculas);
    }

    // ANTI-PADRÃO 6 — erro como HTML/texto em vez de ProblemDetails (RFC 9457):
    // o cliente recebe uma "página" de erro impossível de tratar por código.
    [HttpGet("{id}/matriculas/{matriculaId}")]
    public async Task<IActionResult> GetMatricula(int id, int matriculaId)
    {
        var matricula = await db.Matriculas
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.AlunoId == id);
        if (matricula is null) return NotFound();

        return Ok(matricula);
    }
}
