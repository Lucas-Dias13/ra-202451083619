# Resposta

> **Nomes:** Lucas Adriano Souza Dias   **Turma:** 3001   **Data:** 09 / 09 / 2026

## ENDPOINT 01 — POST /api/getAlunos

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/getAlunos
Retorna TODOS os alunos cadastrados (hoje: 12.482 registros).
Resposta: 200 OK + array JSON completo (~9 MB).
Obs. da wiki: "usar POST porque GET não estava funcionando".
```

1. Qual(is) problema(s) de design vocês identificam?  
Utilização de POST ao invés de GET, rota da api não segue o padrão REST e falta de paginação no JSON.

2. Seu redesenho (método + rota + status codes):  
GET /api/alunos?page=1&size=50  
Resposta: 200 OK + JSON paginado.

## ENDPOINT 02 — GET /deletarAluno?id=7

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /deletarAluno?id=7
Remove o aluno do banco de dados.
Resposta: 200 OK + "OK" (mesmo se o aluno não existir).
Obs. da wiki: "dá pra deletar pelo navegador, bem prático".
```

1. Qual(is) problema(s) de design vocês identificam?  
Utilização de GET invés de DELETE, rota da api com nome em português e status sempre retornando 200 mesmo se o aluno não existir.

2. Seu redesenho (método + rota + status codes):  
DELETE /api/alunos/7  
Respostas: 204 No Content se for removido.  
404 Not Found se não existir.

## ENDPOINT 03 — POST /api/alunos (criação)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/alunos
Body: { "nome": "...", "curso": "..." }
Cria o aluno e responde: 200 OK + body "OK".
O app precisa buscar a lista inteira de novo para descobrir o ID gerado.
```

1. Qual(is) problema(s) de design vocês identificam?  
Retorna 200 OK + "OK" sem as informações do recurso criado e é necessário buscar a lista inteira novamente para descobrir o id.

2. Seu redesenho (método + rota + status codes):  
POST /api/alunos  
Respostas: 201 Created + JSON do aluno criado.  
Header Location: /api/alunos/{id}.  
400 Bad Request se faltar campos obrigatórios.

## ENDPOINT 04 — GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12
Retorna os dados da disciplina 12 da matrícula 88.
Para montar a URL o app precisa conhecer 5 IDs diferentes.
Resposta: 200 OK + JSON da disciplina.
```

1. Qual(is) problema(s) de design vocês identificam?  
Grande aninhamento sobre a URL da api.

2. Seu redesenho (método + rota + status codes):  
GET /api/matriculas/88/disciplinas/12  
Respostas: 200 OK + JSON da disciplina.  
404 Not Found se não existir.

## ENDPOINT 05 — GET /api/alunos/7/matriculas (erro)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /api/alunos/7/matriculas
Se o aluno 7 não existe, responde:
200 OK + "<html><b>Erro: aluno nao existe!</b></html>"
O app mobile quebra tentando fazer parse do JSON.
```

1. Qual(is) problema(s) de design vocês identificam?  
Responde 200 OK mesmo estando com erro e trás uma resposta em HTML invés de JSON.

2. Seu redesenho (método + rota + status codes):  
GET /api/alunos/7/matriculas  
Respostas: 200 OK + JSON das matrículas.  
404 Not Found se não existir.

## ENDPOINT 06 — PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5

**Documentação atual (extraída da wiki da EscolaTech):**

```text
PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5
Atualiza SÓ a nota parcial da disciplina, sem body.
Todos os dados vão na query string.
Resposta: 200 OK + "OK".
```

1. Qual(is) problema(s) de design vocês identificam?  
Uso de PUT invés de PATCH, nome da rota em português e dados sendo enviados por query string.

2. Seu redesenho (método + rota + status codes):  
PATCH /api/alunos/7/disciplinas/12  
Respostas: 200 OK + JSON atualizado.  
404 Not Found se aluno ou disciplina não existir.  
400 Bad Request se tiverem dados inválidos.

## DESAFIO

1. A EscolaTech quer lançar mudanças na API sem quebrar o app mobile antigo, que não recebe atualização há 2 anos. Que decisão de design — que falta na API INTEIRA — resolve esse problema? Como ficariam as rotas?  
Para resolver o problema, é necessário adotar versionamentos a api, para que o mobile continue usando a versão antiga e os novos sistemas utilize a versão mais nova da api. Exemplos:  
Versão antiga: /api/v1/alunos  
Versão nova: /api/v2/alunos
