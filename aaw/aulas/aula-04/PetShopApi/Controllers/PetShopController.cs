using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PetShopApi.Data;
using PetShopApi.Models;

namespace PetShopApi.Controllers;

/// <summary>
/// API do PetHouse.
///
/// ATENÇÃO, ALUNO: esta API FUNCIONA. Toda requisição devolve resposta e nada
/// quebra em produção — e é exatamente por isso que ela passou na revisão de código.
///
/// Só que CADA endpoint aqui viola UMA regra ou diretriz REST vista em aula.
/// Doze endpoints, doze erros diferentes. Sua missão:
///   1. chamar cada um (Postman, curl ou Swagger);
///   2. observar URI, método, status code, headers e corpo da resposta;
///   3. nomear o erro e escrever como você redesenharia.
///
/// Compare sempre com a API do Café Newton (porta 5301), que faz tudo certo.
/// </summary>
[ApiController]
[Produces("application/json")]
public class PetShopController : ControllerBase
{
    private readonly PetShopStore _store;

    // Guardado entre requisições, do jeito mais simples possível.
    private static string? _usuarioDaVez;
    private static int _tutorDaVez;

    public PetShopController(PetShopStore store)
    {
        _store = store;
    }

    // ============================================================ ENDPOINT 01
    /// <summary>Lista os 50 primeiros pets para a tela inicial do aplicativo.</summary>
    [HttpGet("api/v1/pets")]
    public ActionResult<Pagina<Pet>> ListarPets(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        if (page < 1 || size < 1 || size > 100)
        {
            return Problem(
                title: "Parâmetro de paginação inválido.",
                detail: "'page' começa em 1 e 'size' deve estar entre 1 e 100.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var total = _store.Pets.Count;
        var itens = _store.Pets.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new Pagina<Pet> { Page = page, Size = size, Total = total, Items = itens });
    }

    // ============================================================ ENDPOINT 02
    /// <summary>Exclui um pet do cadastro.</summary>
    /// <remarks>Usado pelo botão "remover" da tela de cadastro.</remarks>
    [HttpDelete("api/v1/pets/{id:int}")]
    public IActionResult RemoverPet(int id)
    {
        if (!_store.RemoverPet(id))
        {
            return Problem(
                title: "Pet não encontrado.",
                detail: $"Não existe pet com id {id}.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/pets/{id}");
        }

        return NoContent();
    }

    // ============================================================ ENDPOINT 03
    /// <summary>Consulta a ficha de um pet.</summary>
    [HttpGet("api/v1/pets/{id:int}", Name = "ObterPet")]
    public ActionResult<Pet> ObterPet(int id)
    {
        var pet = _store.BuscarPet(id);
        if (pet is null)
        {
            return Problem(
                title: "Pet não encontrado.",
                detail: $"Não existe pet com id {id}.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/pets/{id}");
        }

        return Ok(pet);
    }

    // ============================================================ ENDPOINT 04
    /// <summary>Lista os atendimentos de banho e tosa.</summary>
    [HttpGet("api/v1/banhos-e-tosas")]
    public ActionResult<Pagina<BanhoTosa>> ListarBanhosETosas(
        [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var itens = _store.BanhosETosas.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new { page, size, total = _store.BanhosETosas.Count, items = itens });
    }

    /// <summary>Lista os tutores do programa de fidelidade.</summary>
    [HttpGet("api/v1/tutores")]
    public ActionResult<Pagina<Tutor>> ListarTutores(
        [FromQuery] bool? vip = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        IEnumerable<Tutor> consulta = _store.Tutores;
        if (vip is not null) consulta = consulta.Where(t => t.Vip == vip.Value);

        var todos = consulta.ToList();
        var itens = todos.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new Pagina<Tutor> { Page = page, Size = size, Total = todos.Count, Items = itens });
    }

    // ============================================================ ENDPOINT 05
    /// <summary>Cadastra um pet novo.</summary>
    [HttpPost("api/v1/pets")]
    [ProducesResponseType(typeof(Pet), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Pet> CadastrarPet([FromBody] Pet pet)
    {
        var criado = _store.CriarPet(pet);

        return CreatedAtAction(nameof(ObterPet), new { id = criado.Id }, criado);
    }

    // ============================================================ ENDPOINT 06
    /// <summary>Consulta um pet pelo id (endpoint usado pelo app mobile).</summary>
    [HttpGet("api/v1/pets/{id:int}", Name = "ObterPet")]
    [ProducesResponseType(typeof(Pet), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Pet> ObterPet(int id)
    {
        var pet = _store.BuscarPet(id);
        if (pet is null)
        {
            return Problem(
                title: "Pet não encontrado.",
                detail: $"Não existe pet com id {id}.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/pets/{id}");
        }

        return Ok(pet);
    }

    // ============================================================ ENDPOINT 07
    /// <summary>Lista de pets consumida pelo aplicativo antigo (contrato de 2024).</summary>
    /// <remarks>
    /// O campo "nome" foi renomeado para "nomeDoPet" no último release para
    /// combinar com o vocabulário do time de produto.
    /// </remarks>
    [HttpGet("api/v1/pets")]
    public ActionResult<Pagina<PetV1>> ListarPetsV1([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var itens = _store.Pets.Skip((page - 1) * size).Take(size)
            .Select(p => new PetV1 { Id = p.Id, Nome = p.Nome, Especie = p.Especie, Raca = p.Raca })
            .ToList();

        return Ok(new Pagina<PetV1> { Page = page, Size = size, Total = _store.Pets.Count, Items = itens });
    }

    [HttpGet("api/v2/pets")]
    public ActionResult<Pagina<PetV2>> ListarPetsV2([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var itens = _store.Pets.Skip((page - 1) * size).Take(size)
            .Select(p => new PetV2 { Id = p.Id, NomeDoPet = p.Nome, Especie = p.Especie, Raca = p.Raca })
            .ToList();

        return Ok(new Pagina<PetV2> { Page = page, Size = size, Total = _store.Pets.Count, Items = itens });
    }

    // ============================================================ ENDPOINT 08
    /// <summary>Consulta o resultado de um exame.</summary>
    [HttpGet("api/v1/exames/{id:int}", Name = "ObterExame")]
    public ActionResult<Exame> ObterExame(int id)
    {
        var exame = _store.Exames.FirstOrDefault(e => e.Id == id);
        if (exame is null)
        {
            return Problem(
                title: "Exame não encontrado.",
                detail: $"Não existe exame com id {id}.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/exames/{id}");
        }

        return Ok(exame);
    }

    [HttpGet("api/v1/consultas/{consultaId:int}/exames")]
    public ActionResult<List<Exame>> ExamesDaConsulta(int consultaId)
    {
        if (!_store.Consultas.Any(c => c.Id == consultaId))
        {
            return Problem(
                title: "Consulta não encontrada.",
                statusCode: StatusCodes.Status404NotFound,
                instance: $"/api/v1/consultas/{consultaId}");
        }

        return Ok(_store.Exames.Where(e => e.ConsultaId == consultaId).ToList());
    }

    // ============================================================ ENDPOINT 09
    /// <summary>Lista as consultas veterinárias para o relatório da clínica.</summary>
    [HttpGet("api/v1/consultas")]
    public ActionResult<Pagina<Consulta>> ListarConsultas(
        [FromQuery] int page = 1,
        [FromQuery] int size = TamanhoPadrao,
        [FromQuery] int? petId = null,
        [FromQuery] string? veterinario = null,
        [FromQuery] string? sort = null)
    {
        if (page < 1 || size < 1 || size > TamanhoMaximo)
        {
            return Problem(
                title: "Parâmetro de paginação inválido.",
                detail: $"'page' começa em 1 e 'size' deve estar entre 1 e {TamanhoMaximo}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        IEnumerable<Consulta> consulta = _store.Consultas;

        // Filtros são query strings DO recurso — nunca endpoints novos
        // como /consultasDoPet ou /buscarConsultaPorVeterinario.
        if (petId is not null)
            consulta = consulta.Where(c => c.PetId == petId.Value);

        if (!string.IsNullOrWhiteSpace(veterinario))
            consulta = consulta.Where(c => c.Veterinario.Contains(veterinario, StringComparison.OrdinalIgnoreCase));

        consulta = sort switch
        {
            "data" => consulta.OrderBy(c => c.Data),
            "-data" => consulta.OrderByDescending(c => c.Data),
            _ => consulta.OrderBy(c => c.Id)
        };

        var todos = consulta.ToList();
        var itens = todos.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new Pagina<Consulta>
        {
            Page = page, Size = size, Total = todos.Count, Items = itens
        });
    }

    // ============================================================ ENDPOINT 10
    /// <summary>Registra a carteira de vacinação do pet.</summary>
    [HttpPost("api/v1/pets/{id:int}/vacinas")]
    public ActionResult<Vacina> AplicarVacina(int id, [FromBody] Vacina vacina)
    {
        if (_store.BuscarPet(id) is null)
        {
            return Problem(title: "Pet não encontrado.",
                           statusCode: StatusCodes.Status404NotFound,
                           instance: $"/api/v1/pets/{id}");
        }

        var criada = _store.RegistrarVacina(id, vacina.Nome);

        // 201 + Location do recurso criado (ver gabarito 05)
        return CreatedAtAction(nameof(ObterVacina), new { id, vacinaId = criada.Id }, criada);
    }

    [HttpPut("api/v1/pets/{id:int}/vacinas")]
    public ActionResult<List<Vacina>> SubstituirCarteira(int id, [FromBody] List<Vacina> carteira)
    {
        if (_store.BuscarPet(id) is null)
        {
            return Problem(title: "Pet não encontrado.",
                           statusCode: StatusCodes.Status404NotFound,
                           instance: $"/api/v1/pets/{id}");
        }

        _store.SubstituirCarteira(id, carteira);    // apaga as antigas e grava as enviadas
        return Ok(_store.VacinasDoPet(id));
    }

    // ============================================================ ENDPOINT 11
    /// <summary>Autentica o tutor no aplicativo.</summary>
    [HttpPost("api/v1/sessoes")]
    public ActionResult<TokenEmitido> Autenticar([FromBody] Credenciais credenciais)
    {
        var tutor = _autenticador.Validar(credenciais);
        if (tutor is null) return Unauthorized();

        // Na aula 07 isso vira um JWT assinado de verdade.
        var token = _emissorDeTokens.Emitir(tutor);

        return Ok(new TokenEmitido { AccessToken = token, ExpiraEm = 3600 });
    }

    /// <summary>Lista os pets do tutor autenticado.</summary>
    [Authorize]
    [HttpGet("api/v1/pets")]
    public ActionResult<Pagina<Pet>> MeusPets([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var tutorId = int.Parse(User.FindFirst("tutorId")!.Value);   // veio do token
        var todos = _store.Pets.Where(p => p.TutorId == tutorId).ToList();
        var itens = todos.Skip((page - 1) * size).Take(size).ToList();

        return Ok(new Pagina<Pet> { Page = page, Size = size, Total = todos.Count, Items = itens });
    }

    // ============================================================ ENDPOINT 12
    /// <summary>Tabela de preços dos serviços (reajustada uma vez por ano).</summary>
    [HttpGet("api/v1/tabela-de-precos")]
    [ProducesResponseType(typeof(List<ItemDePreco>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public IActionResult TabelaDePrecos()
    {
        // A versão sobe quando o preço é reajustado — é a identidade do conteúdo.
        var etag = $"\"precos-v{_store.VersaoDaTabelaDePrecos}\"";

        // O cliente já tem esta versão? Devolve 304, sem corpo, sem custo.
        var enviadoPeloCliente = Request.Headers[HeaderNames.IfNoneMatch].ToString();
        if (enviadoPeloCliente.Split(',').Select(v => v.Trim()).Contains(etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers[HeaderNames.ETag] = etag;
        Response.Headers[HeaderNames.CacheControl] = "public, max-age=3600";

        return Ok(_store.TabelaDePrecos);
    }
}
