# Respostas

> **Nomes:** Lucas Adriano Souza Dias   **Turma:** 3001   **Data:** 12 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF  
Estilo Síncrono.

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):  
Cliente → Serviço de Pagamentos → Serviço de Contas → Serviço de Pagamentos → Cliente

3. Justificativa (mínimo 2 fatores):  
O cliente espera a resposta imediata para concluir a compra e sem a resposta do servidor de contas não toma decisões.

4. Principal risco da escolha:  
Se o serviço de Contas ficar indisponível, o checkout trava e o cliente não consegue pagar.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF  
Estilo Assíncrono (fila/evento).

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):  
Cliente → Serviço de Cadastro → Fila de Mensagens → Serviço de E-mail → Provedor de E-mail → Cliente (recebe e-mail depois)

3. Justificativa (mínimo 2 fatores):  
O usuário pode começar a usar o app sem esperar o envio do e-mail e o envio pode ser reprocessado em caso de falha.

4. Principal risco da escolha:  
Mensagens podem acumular em picos e atrasar muito o envio se o sistema de filas não escalar bem.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF  
Estilo Assíncrono (fila/evento).

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):  
Cliente → Serviço de Checkout → Fila de Mensagens → Serviço de Estoque

3. Justificativa (mínimo 2 fatores):  
Checkout não fica lento, pois não depende da resposta imediata do Estoque e a fila garante que nenhuma baixa seja perdida, mesmo em picos de tráfego.

4. Principal risco da escolha:  
Se houver duplicidade de mensagens ou falha no consumidor, pode ocorrer baixa incorreta.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF  
Estilo API Gateway/BFF.

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):  
Cliente (App) → API Gateway/BFF → Serviços (Saldo, Cartão, Investimentos, Empréstimos, Cashback)

3. Justificativa (mínimo 2 fatores):  
Reduz chamadas múltiplas e o Gateway adapta formatos e autenticação.

4. Principal risco da escolha:  
Se o Gateway cair, todos os serviços ficam inacessíveis.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?  
Um dos cenários foi no MegaMarket, o estoque poderia ser baixado duas vezes para a mesma venda. Para se proteger neste caso, cada mensagem deveria ter um ID único  e o serviço de estoque deveria verificar se já processou o ID  daquela mensagem antes de aplicar a baixa.
