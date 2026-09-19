# Respostas

> **Nomes:** Lucas Adriano Souza Dias   **Turma:** 3001   **Data:** 19 / 09 / 2026

## VULNERABILIDADE 01 — A busca de clientes

> `GET /api/clientes/buscar?nome=...`

Endpoint de busca usado pela tela de atendimento. O parâmetro nome vem direto da caixa de busca do site.

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var sql = "SELECT * FROM Clientes WHERE Nome = '"
 5                + nome + "'";
 6      var clientes = _db.Clientes.FromSqlRaw(sql).ToList();
 7      return Ok(clientes);
 8  }
```

**Sua análise:**

1. Qual é a falha?  
Concatenação do parâmetro "nome" dentro da consulta.

2. Qual o dano possível em produção?  
Roubo de dados, exclusão ou alteração de registros indevidos e comprometimento do banco de dados.

3. Como corrigir?  
Não utilizar concatenação nas informações vindas de usuários e sim utilizar consultas parametrizadas.

## VULNERABILIDADE 02 — A consulta de faturas

> `GET /api/faturas/{id}`

Endpoint usado pelo app para exibir a fatura do cartão. O usuário está autenticado quando chama esta rota.

```text
 1  [HttpGet("{id}")]
 2  public IActionResult GetFatura(int id)
 3  {
 4      var fatura = _db.Faturas.Find(id);
 5      if (fatura == null) return NotFound();
 6      return Ok(fatura);
 7  }
```

**Sua análise:**

1. Qual é a falha?  
O código não possui autenticação do usuário que está consultando, podendo fazer consultas nas faturas de outros usuários.

2. Qual o dano possível em produção?  
Vazamento de informações, quebra de sigilo bancário e possíveis fraudes.

3. Como corrigir?  
Verificar se a fatura realmente pertence aquele usuário que está tentando consultar, através do ID de usuário.

## VULNERABILIDADE 03 — A configuração do servidor

> `Program.cs (roda igual em dev e em produção)`

Trecho de inicialização da API, idêntico em todos os ambientes. Este arquivo está versionado no Git da empresa.

```text
 1  public const string Conn =
 2      "Server=prod-db;Database=Banco;User=sa;" +
 3      "Password=Newton@2026!";
 4
 5  var app = WebApplication.CreateBuilder(args).Build();
 6  app.UseDeveloperExceptionPage();
 7  app.Run();
```

**Sua análise:**

1. Qual é a falha?  
As informações de acesso ao banco de dados está exposta e versionada no Git e a página de erros para desenvolvedores está ativa em produção.

2. Qual o dano possível em produção?  
Qualquer usuário pode ter acesso ao banco de dados e acesso a estrutura do código, já que fica explicito caso ocorra alguma falha em execução.

3. Como corrigir?  
Não versionar senhas, usar variáveis de ambientes e desabilitar a página de erros para desenvolvedores quando estiver em produção.

## VULNERABILIDADE 04 — A atualização de perfil

> `PUT /api/usuarios/{id}`

Endpoint que o app chama quando o usuário edita o próprio perfil. O corpo da requisição é o JSON enviado pelo cliente.

```text
 1  public class UsuarioUpdate
 2  {
 3      public string Nome  { get; set; }
 4      public string Email { get; set; }
 5      public string Role  { get; set; }   // "user" | "admin"
 6  }
 7
 8  [HttpPut("{id}")]
 9  public IActionResult Atualizar(int id, UsuarioUpdate dto)
10  {
11      _repo.AtualizarTudo(id, dto);
12      return NoContent();
13  }
```

**Sua análise:**

1. Qual é a falha?  
Ter um campo no qual é possivel qualquer usuário alterar sua permissão de acesso para "admin".

2. Qual o dano possível em produção?  
Qualquer usuário pode se tornar administrador, podendo ter acesso total na aplicação e resultando em acesso indevido aos dados.

3. Como corrigir?  
Remover a linha do "Role", para que não seja um dado manipulável via JSON.

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?  
A falha 2, já que se trata de uma falha na lógica da implementação de autorização e não da funcionalidade do código.

