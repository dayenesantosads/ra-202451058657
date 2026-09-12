# Atividade — AULA 06

## Síncrono ou Assíncrono?

_Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

_⏱️ Tempo: 25 minutos | 👥 Formato: em duplas | Não existe resposta única — o que vale é a justificativa._

> **Nomes:** Dayene dos Santos Rosa | **Turma:** GNP0547 - 3001 | **Data:** 12 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1.  Estilo recomendado: [x] Síncrono ☐ Assíncrono (fila/evento) ☐ API Gateway/BFF

2.  Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
            [Cliente / Checkout]
                      |
                      V (1) POST/pagamentos (finalizar compra)
            [Serviço de Pagamentos] -----------------------------> [Serviço de Contas]
                                      (2) GET/contas/{id}/saldo
            [Serviço de Pagamentos] <----------------------------- [Serviço de Contas]
                      |               (3) Resposta com saldo/limite (~80 ms)
                      V
            (4) Resposta HTTP 200 OK (Aprovado) ou 422 Unprocessable Entity (Negado)
```

3.  Justificativa (mínimo 2 fatores):
    - Decisão em tempo real: O cliente está na tela de checkout esperando a confirmação da compra.
      Por isso, o sistema precisa receber uma resposta rapidamente para informar se a compra foi aprovada ou não.
    - Necessidade de confirmação imediata: O sistema não pode aprovar a compra sem saber se existe saldo disponível.
      Então, ele precisa consultar o Serviço de contas e esperar a resposta antes de tomar uma decisão.
      Por esse motivo, não seria adequado trabalhar com uma confirmação que chegasse somente depois.
    - Baixa latência: O Serviço de contas normalmente responde em aproximadamente 80 ms. Como esse tempo é bem baixo,
      é possível fazer uma chamada HTTP sincrona sem causar uma demora significativa para o usuário.

4.  Principal risco da escolha:
    - Acoplamento temporal e propagação de falhas: Se o Serviço de contas estiver fora do ar ou demorar muito para responder,
      o Serviço de Pagamentos não vai conseguir confirmar a transação. Com isso, a compra pode não ser concluída e o cliente
      pode acabar recebendo um erro no checkout.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1.  Estilo recomendado: ☐ Síncrono [x] Assíncrono (fila/evento) ☐ API Gateway/BFF

2.  Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
            [Usuário] --(1) POST/cadastros --> [Serviço de Cadastro] -- (2) Publica evento "NovoUsuarioCadastrado" --> [Fila / Broker (RabbitMQ/Kafka)]
                |                                  |
                |<--(3) 201 Created (Imediato) ----|
                                                                                                                                  |
                                                                                                                                  V (4) Consome mensagem
                                                                                                                        [Serviço de Notificações]
                                                                                                                                  |
                                                                                                                                  V (5) Envia email (até 8s)
                                                                                                                        [Provedor de Email (Externo)]
```

3.  Justificativa (mínimo 2 fatores):

- Não precisa ser imediato: O usuário pode usar o aplicativo normalmente mesmo que o email demore alguns segundos ou até um pouco mais para chegar.
  Então, não é necessário que o envio aconteça na mesma hora.

- Evita que uma falha atrapalhe o cadastro: Como o serviçoexterno demora cerca de 8 segundos e pode apresentar falhas, em algumas tentativas,
  usar uma fila permite que o cadastro continue normalmente. O sistema pode tentar enviar o email novamente depois, sem travar o usuário.

4. Principal risco da escolha:
   - Atraso na entrega do email: Se houver muitas mensagens na fila ou algum problema no serviço que envia os emails, o usuário pode demorar
     mais para receber a mensagem. Isso pode ser um problema se ele precisar desse email para continuar o cadastro ou receber alguma instrução inicial.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1.  Estilo recomendado: ☐ Síncrono [x] Assíncrono (fila/evento) ☐ API Gateway/BFF

2.  Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
           [Cliente / Checkout] --(1) POST/pedidos --> [Serviço de Vendas] -- (2) Publica evento "Venda Realizada" --> [Fila / Broker de Mensagens]
                    |                                           |
                    |<--(3) 202 Accepted (Venda Registrada) ----|
                                                                                                                                  |
                                                                                                                                  V (4) Consome no seu ritmo
                                                                                                                        [Serviço de Estoque]
                                                                                                                                  |
                                                                                                                                  V (5) Atualiza Saldo
                                                                                                                        [Banco de Estoque]

```

3.  Justificativa (mínimo 2 fatores):
    - Ajuda em momento de muitos acessos: Durante uma promoção, o número de compras pode aumentar bastante.
      A fila consegue guardar essas solicitações para que o Serviço de Estoque consiga processá-las aos poucos,
      sem ficar sobrecarregado.
    - Evita perder as mensagens: As informações fica armazenadas na fila. Assim, mesmo que o Serviço de estoque
      esteja sobrecarregado ou tenha algum problema, as solicitações não são simplesmente perdidas.

    - Não deixa o checkout esperando: O checkout pode responder rapidamente com o status 202 Accepted, informando
      que a solicitação foi recebida. Dessa forma, o cliente não precisa ficar esperando o Serviço de estoque
      terminar o processamento.

4.  Principal risco da escolha:
    - Risco de vender um produto que já acabou: Como a atualização do estoque acontece depois da compra, pode existir
      um pequeno atraso. Nesse intervalo, duas pessoas podem comprar o mesmo produto quando resta apenas uma unidade,
      fazendo com que o sistema aceite uma venda sem ter estoque suficiente.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1.  Estilo recomendado: ☐ Síncrono ☐ Assíncrono (fila/evento) [x] API Gateway/BFF

2.  Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
           [App Mobile]
                |
                V (1) GET/api/v1/dashboard-mobile (Única chamada HTTP)
           [BFF Mobile (Backend para Frontend)]
                |
                +----(2a) GET/saldo------------> [Serviço de Saldo]
                +----(2b) GET/fatura-----------> [Serviço de Cartões]
                +----(2a) GET/investimentos----> [Serviço de Investimentos]
                +----(2a) GET/emprestimos------> [Serviço de Empréstimos]
                +----(2a) GET/cashback---------> [Serviço de Cashback]
                |
                V (3) Agrega as 5 respostas e formata no JSON exato que o app precisa
           [App Mobile] <-------------------(4) Resposta HTTP 200 OK consolidada

```

3.  Justificativa (mínimo 2 fatores):
    - Reduz a quantidade de chamadas: Em uma rede móvel que pode ser mais lenta, o aplicativo
      precisaria fazer várias chamadas para buscar as informações. Com o BFF, essas informações
      podem ser reunidas em uma única chamada, deixando o acesso mais simples e rápido.
    - Atende melhor cada tipo de aplicativo: O BFF pode organizar as informações de acordo com
      a necessidade do aplicativo mobile. No futuro, poderia existir outro BFF para a versão Web,
      sem precisar alterar os serviços internos.

4.  Principal risco da escolha:
    - Pode virar um ponto de falaha: O BFF passa a ser uma parte importante do sistema. Se ele parar
      de funcionar ou tiver problemas para acessar algum serviço interno, a tela inicial do aplicativo
      pode não conseguir carregar as informações.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?

Cenário 03 - MegaMarket (Baixa de estoque nos picos)

- 1. O que aconteceria se a mensagem fosse entregue duas vezes?

     Em alguns casos, o broker pode entregar a mesma mensagem mais de uma vez. Isso pode acontecer,por exemplo, se ocorrer algum problema de comunicação depois que a mensagem já foi processada. Nesse caso, o evento:

     `VendaRealizada(IdVenda: 9876, ProdutoID: 45, Qtd: 1)`

     poderia chegar duas vezes ao serviço de estoque.
     Se o serviço não verificar isso, ele poderia diminuir o estoque duas vezes, mesmo sendo apenas uma venda.Isso faria o estoque ficar incorreto.

- 2.  Como o consumidor pode evitar esse problema?

          O serviço de estoque precisa ser idempotente. Isso significa que, mesmo que a mesma mensagem seja recebida mais de uma vez, o resultado final deve ser o mesmo. Uma forma de fazer isso seria:

            I. Guardar as vendas que já foram processadas: Cada venda teria um identificador, como o `idVenda` .
               Antes de dar baixa no estoque, o serviço verificaria se aquela venda já foi processada.

            II. Evitar o mesmo ID duas vezes: O banco de dados pode impedir que o mesmo `idVenda` seja registrado novamente.
                Assim, se a mensagem chegar pela segunda vez, o sistema identifica que aquela venda já foi processada e não
                realiza uma nova baixa no estoque.

          Dessa forma, mesmo que a mesma mensagem seja recebida duas vezes, o estoque será diminuído apenas uma vez.
