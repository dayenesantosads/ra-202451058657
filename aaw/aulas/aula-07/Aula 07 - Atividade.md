# HANDOUT — AULA 07

## Caça às Vulnerabilidades

_Revisão de segurança de uma API .NET — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês são a dupla de revisores de segurança da empresa. Os 4 trechos abaixo são da MESMA API, prestes a ir para produção. Para CADA card:

- Descrevam a falha com as próprias palavras (não precisa do nome técnico ainda)
- Estimem o dano possível se isso chegar à produção
- Proponham a correção

_⏱️ Tempo: 30 minutos | 👥 Formato: em duplas | Todo o código é fictício e roda apenas no laboratório._

> **Nomes:** Dayene dos Santos Rosa | **Turma:** GNP0547 - 3001 | **Data:** 18 / 09 / 2026

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

- A entrada do usuário é colocada diretamente na consulta SQL. Isso pode permitir que alguém coloque comandos SQL no campo de busca.

2. Qual o dano possível em produção?

- Um hacker pode conseguir acessar, alterar ou excluir dados do banco de dados.

3. Como corrigir?

- Usar consultas parametrizadas, como `FromSqlInterpolated` , para que o valor digitado seja tratado apenas como dado.

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

- O sistema verifica se a fatura existe, mas não verifica se ela pertence ao usuário que está logado.

2. Qual o dano possível em produção?

- Um usuário pode trocar o id da fatura na URL e conseguir visualizar dados financeiros de outra pessoa.

3. Como corrigir?

- Verificar se o `ClienteId` da fatura é igual ao usuário logado. Caso não seja, retornar **403 Forbidden** (_O servidor entendeu o pedido, mas está recusando o acesso_.).

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

- A senha do banco está escrita diretamente no código e a página de erros detalhados está ativa também em produção.

2. Qual o dano possível em produção?

- A senha pode ser descoberta pelo código ou histórico do git. Além disso, os erros podem mostrar informações internas do sistema.

3. Como corrigir?

- Guardar as credenciais em variáveis de ambiente ou ferramentas de configuração e deixar os erros detalhados apenas no ambiente de desenvolvimento.

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

- O usuário consegue enviar o campo `Role` na atualização do perfil, podendo tentar alterar sua própria permissão.

2. Qual o dano possível em produção?

- Um usuário comum poderia tentar se tornar administrador.

3. Como corrigir?

- Criar um `DTO` (_Data Transfer Object (Objeto de Transferência de Dados) é um objeto usado para transportar somente os dados que precisam entrar ou sair de uma parte do sistema._) que permita alterar somente os dados necessários, como `Nome` e `Email`, sem incluir o campo _Role_.

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?
   - VULNERABILIDADE 02 — A consulta de faturas.
   - Porque o código pode estar funcionando normalmente e até exigir login. O problema está na regra de negócio, o sistema precisa saber se aquela fatura pertence ao usuário logado.
     Um scanner consegue encontrar mais facilmente problemas como senha exposta, SQL montado de forma insegura ou campos administrativos no formulário. Já a verificação de quem é o dono do dado pode exigir uma análise mais próxima da lógica da aplicação.
