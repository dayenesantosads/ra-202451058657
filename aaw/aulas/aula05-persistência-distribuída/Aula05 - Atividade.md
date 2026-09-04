# HANDOUT — AULA 05

## Escolha o Banco

*Persistência em arquiteturas distribuídas — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são o time de arquitetura de dados contratado pelas 4 empresas abaixo. Para CADA cenário:

- Escolham o modelo de banco: relacional, documento, chave-valor ou grafo
- Justifiquem com pelo menos 2 fatores do contexto (estrutura dos dados, padrão de acesso, escala, consistência...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** Dayene dos Santos Rosa - Luiz Felipe Vieira de Paula | **Turma:** GNP0547 - 3001  | **Data:** 03 / 09 / 2026

## CENÁRIO 01 — TechStore — o catálogo camaleão

E-commerce com 80 mil produtos. Cada categoria tem atributos completamente diferentes: livro tem autor e número de páginas; notebook tem RAM e CPU; camiseta tem tamanho e cor.

- A cada categoria nova, o time faz ALTER TABLE e a tabela produtos já tem 92 colunas (a maioria NULL)
- O produto é quase sempre lido INTEIRO, de uma vez, para montar a página
- Novos atributos surgem toda semana — o marketing não espera o DBA
- Relatórios cruzando categorias são raros

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     x Documento     ☐ Chave-valor     ☐ Grafo
   - Modelo Documento

2. Justificativa (mínimo 2 fatores do contexto):
   - Estrutura de dados flexiveis para futuras atualizações e incrementações
   - Evita alteração em toda a estrutura ao adicionar um atributo específico em um item

3. Principal risco da escolha:
   - Inconsistências e complexidade de busca ou filtragem de algum dado específico.

## CENÁRIO 02 — MegaCart — o carrinho da Black Friday

Serviço de carrinho de compras de um varejista gigante. Na Black Friday são milhões de leituras e escritas por minuto.

- O acesso é SEMPRE pela chave: “carrinho do cliente 12345” — nunca por busca ou filtro
- Todo carrinho expira automaticamente em 48h (TTL)
- Latência precisa ser de poucos milissegundos
- Perder um carrinho é chato, mas NÃO é tragédia — o cliente remonta

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     x Chave-valor     ☐ Grafo
   - Modelo Chave-valor

2. Justificativa (mínimo 2 fatores do contexto):
   - Busca direcionada/específica através de palavras chaves ou id's.
   - É o melhor modelo se tratando de sessões, velocidade de busca e armazenamento temporario de dados.

3. Principal risco da escolha:
   - É a sua limitação para consultas complexas e relacionamentos

## CENÁRIO 03 — PayBank — dinheiro não pode evaporar

Módulo de transferências de um banco. Uma transferência debita uma conta e credita outra — as duas operações têm que acontecer JUNTAS ou nenhuma acontece.

- Consistência forte exigida por lei — saldo errado é multa do Banco Central
- Auditoria cruza contas, clientes, agências e transações em relatórios complexos (joins)
- O esquema dos dados é estável há 10 anos
- Volume alto, mas previsível

**Sua análise:**

1. Modelo recomendado:   x Relacional     ☐ Documento     ☐ Chave-valor     ☐ Grafo
   - Modelo relacional ACID

2. Justificativa (mínimo 2 fatores do contexto):
   - Utilização do modelo ACID para assertividade das transações necessárias e em caso de falha, que ela seja revertida antes de gerar problemas.
   - Consistência nas transações e estabilidade para que não haja movimentações paralelas, gerando maior segurança ao sistema.

3. Principal risco da escolha:
   - Maior dificuldade de aplicar de mudanças e alto custo de escalabilidade

## CENÁRIO 04 — FriendLink — amigos dos seus amigos

Rede social profissional em que o produto principal é a indicação: “pessoas que você talvez conheça” e “quem pode te apresentar à empresa X”.

- As consultas dominantes percorrem RELACIONAMENTOS: amigos dos amigos, caminhos de indicação com até 6 níveis
- Em banco relacional, cada nível vira um self-join — com 6 níveis a consulta já não responde
- Os dados de perfil são simples; o valor está nas CONEXÕES
- O grafo cresce milhões de arestas por dia

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     ☐ Chave-valor     x Grafo
    - Modelo Grafo
2. Justificativa (mínimo 2 fatores do contexto):
    -  Foca nos relacionamentos e conexões dos dados e não nos atributos
    - Facilidade para percorrer vários relacionamentos
3. Principal risco da escolha:
   - Maior complexidade de manutenção conforme a escalabilidade de dados

## DESAFIO

1. Escolha um dos cenários e responda: se a rede particionar (metade dos servidores não enxerga a outra metade), o que o sistema deve fazer — parar de responder para não errar, ou continuar respondendo mesmo arriscando dados desatualizados? Qual letra do CAP vocês sacrificariam e por quê?
   - CENÁRIO 03 — PayBank — dinheiro não pode evaporar, nesse cenário o sistema deve parar de responder para não errar.
   - Sacrificaria a letra A (Availability - sistema sempre responde), para evitar a efetivação de uma transação incorretamente por inconsistencias no sistema.