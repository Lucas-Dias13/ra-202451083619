# Respostas

> **Nomes:** Lucas Adriano Souza Dias   **Turma:** 3001   **Data:** 09 / 09 / 2026

## REQUISIÇÃO 01 — A prateleira inteira

```text
→ REQUISIÇÃO
GET /api/livros HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

[ { "id": 1, "titulo": "Clean Code", "autor": "Robert C. Martin" },
  { "id": 7, "titulo": "O Programador Pragmático", "autor": "Hunt & Thomas" } ]
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para listar todos os livros.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "200 OK", informando que deu certo.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?  
O estado do servidor não muda, a resposta continua a mesma.

## REQUISIÇÃO 02 — O livro fantasma

```text
→ REQUISIÇÃO
GET /api/livros/99 HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{ "title": "Not Found", "status": 404 }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para listar o livro cujo id fosse 99.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "404 Not Found", informando que não deu certo pois não existe. A culpa foi de quem efetuou a requisição, já que buscou por algo inexistente.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?  
O estado do servidor não muda e a resposta continua a mesma.

## REQUISIÇÃO 03 — Livro novo na estante

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

```text
← RESPOSTA
HTTP/1.1 201 Created
Location: /api/livros/8
Content-Type: application/json

{ "id": 8, "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para criar um novo livro na estante.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "201 Created", informando que deu certo e o livro foi criado na estante.

3. Enviando este POST 3 vezes seguidas, o que acontece na estante? Para que serve o header Location?  
Cada POST enviado criará um novo livro na estante. O header location serve para informar qual o id de acesso ao livro.

## REQUISIÇÃO 04 — Corrigindo a ficha completa

```text
→ REQUISIÇÃO
PUT /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para atualizar o livro de id 7.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "200 OK", informando que a atualização deu certo.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?  
O estado do servidor não muda e a resposta continua a mesma.

## REQUISIÇÃO 05 — Fora do catálogo

```text
→ REQUISIÇÃO
DELETE /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
```

```text
← RESPOSTA
HTTP/1.1 204 No Content
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para remover o livro com id 7.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "204 No Content", informando que foi removido o livro.

3. Repetindo o DELETE, o estado do servidor muda? Que resposta você ESPERA na segunda vez?  
O estado do servidor muda somente na primeira vez que é efetuado o DELETE, após as repetições não há qualquer mudança.

## REQUISIÇÃO 06 — O cadastro capenga

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "autor": "Anônimo" }
```

```text
← RESPOSTA
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{ "title": "Bad Request", "status": 400,
  "errors": { "Titulo": [ "O campo Titulo é obrigatório" ] } }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?  
O cliente pediu para criar um livro, contendo somente a informação do autor.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?  
O status foi "400 Bad Request", informando que houve um erro de validação. A culpa foi do cliente, já que fez uma requisição sem informar o titulo do livro, que era algo obrigatório.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?  
O estado do servidor não muda e a resposta continua a mesma.

## TABELA-SÍNTESE — Os verbos do HTTP

| **Verbo** | **Para que serve** | **Seguro?** | **Idempotente?** | **Status típicos** |
| --- | --- | --- | --- | --- |
| **`GET`** | Consultar recursos | Sim | Sim | 200, 404 |
| **`POST`** | Criar recursos | Não | Não | 201, 400 |
| **`PUT`** | Atualizar recurso inteiro | Não | Sim | 200, 404 |
| **`PATCH`** | Atualizar recurso parcialmente | Não | Não | 200, 204, 404 |
| **`DELETE`** | Remover recurso | Não | Sim | 204, 404 |

## DESAFIO

1. O verbo PATCH não apareceu em nenhum card. Qual a diferença entre PATCH e PUT? Um app de banco quer alterar SÓ o apelido do usuário, entre dezenas de campos do perfil — qual dos dois você usaria e por quê?  
O verbo PUT altera o recurso inteiro, precisando de todos os campos serem preenchidos, já o verbo PATCH altera parcialmente o recurso, podendo preencher somente os campos a serem alterados. Eu utilizaria o PATCH, pois seria desnecessário e trabalhoso alterar todas as informações sendo que a intenção é somente atualizar um campo específico.
