\# Trabalho Semestral · Arquitetura de Aplicações Web 2026.2





\* \*\*Aluno:\*\* Dayene Dos Santos Rosa

\* \*\*Curso/Turma:\*\* Análise e Desenvolvimento de Sistemas | GNP0547 - 3001

\* \*\*Repositório:\*\* \[GitHub/DayeneSantos](https://github.com/dayenesantosads/ra-202451058657)



\## Proposta



\## 1. Domínio e problema

GeekStore é uma loja virtual especializada em produtos geek (action figures, jogos de tabuleiro, quadrinhos e colecionáveis). A aplicação resolve o problema de gerenciar o catálogo de produtos e o ciclo de vida dos pedidos de forma centralizada, com controle de estoque em tempo real, evitando a venda de itens indisponíveis. É voltada tanto para quem administra a loja (gerenciar catálogo e acompanhar vendas) quanto para o cliente (consultar produtos e realizar pedidos).

\## 2. Entidades

\### Entidade A — Produto:

&#x20;   id, nome, descricao, categoria, preco, estoque, imagemUrl, criadoEm.



\### Entidade B — Pedido:

&#x20;   id, clienteNome, itens (lista de {produtoId, nomeProduto, quantidade, precoUnitario}), total, status (Criado, Pago, Enviado, Cancelado), criadoEm.

\### Relacionamento e modelagem NoSQL:

Existe uma relação de 1 para muitos (1-N) entre um Pedido e seus itens. Os itens ficam dentro do próprio pedido, porque eles só existem como parte de um pedido e não precisam ser consultados separadamente.



Por isso, é mais adequado guardar os itens dentro do pedido do que criar uma coleção separada para eles.



Além disso, cada item guarda o nome e o preço do produto no momento da compra. Dessa forma, mesmo que o produto seja alterado ou excluído depois, o pedido continua mostrando corretamente o que foi comprado e quanto custava na época..

\## 3. Endpoints previstos

\* Produtos

&#x20;   - GET /produtos — lista todos os produtos (filtro opcional por categoria)

&#x20;   - GET /produtos/{id} — busca um produto por id

&#x20;   - POST /produtos — cria um produto

&#x20;   - PUT /produtos/{id} — atualiza um produto

&#x20;   - DELETE /produtos/{id} — remove um produto



\* Pedidos

&#x20;   - GET /pedidos — lista todos os pedidos

&#x20;   - GET /pedidos/{id} — busca um pedido por id

&#x20;   - POST /pedidos — cria um pedido (aplica a regra de negócio da seção 4)

&#x20;   - PATCH /pedidos/{id}/status — atualiza o status do pedido

&#x20;   - DELETE /pedidos/{id} — cancela/remove um pedido



\- Autenticação (Bônus A/B)

&#x20;   - POST /auth/registrar — cria um usuário (perfil 'usuario' por padrão)

&#x20;   - POST /auth/login — autentica e devolve um JWT

\## 4. Regra de negócio

Ao criar um Pedido (`POST /pedidos`), para cada item informado o serviço verifica se o Produto referenciado existe e se há estoque suficiente. Se houver, decrementa o estoque do produto e calcula o `total` do pedido (soma de quantidade × preço unitário). Se qualquer item não tiver estoque suficiente, o pedido inteiro é rejeitado e nenhum estoque é alterado — a validação e a baixa de estoque acontecem na camada de serviço, não no controller.

\## 5. Casos de erro

\- Buscar Produto ou Pedido por `id` inexistente → 404 Not Found

\- Criar Pedido com quantidade maior que o estoque disponível de algum item → 400 Bad Request ("Estoque insuficiente")

\## 6. Stack escolhida

\- \*\*Backend:\*\*  .NET (C#) com ASP.NET Core Web API

\- \*\*Banco:\*\* MongoDB, via docker-compose local

\- \*\*Frontend:\*\* HTML + JavaScript puro (fetch), sem framework

\## 7. Bônus pretendidos

Os quatro bônus:

\- A — JWT e autenticação

\- B — Autorização com perfis (RBAC): `admin` e `usuario`

\- C — Testes unitários da camada de serviço (2 cenários de sucesso + 2 de erro)

\- D — Princípios SOLID documentados (mínimo três dos cinco, explicados na arguição)

