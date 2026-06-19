# FinanceFlow — Planejamento

## Visão
App de controle de gastos da vida + previsão de meses futuros + projeção de investimento/poupança. Mobile-first, instalável (PWA) — "desktop app" e mobile com uma base de código só. Projeto de **aprendizado** do stack que se usa na XP (.NET, React, Kafka, data-driven), com Claude Code no papel de tutor.

## Decisões
- **.NET 10** (alinhado às refs TechfinChallenge e Micro-CRM; toolchain local é net10).
- **Monolito modular** (1 host de API, módulos isolados) — não microsserviços. Migração futura é viável porque os módulos já são isolados.
- **Kafka** para o fluxo data-driven, mas só na Fase 2. Na Fase 1 o `IEventBus` (no SharedKernel) usa um `LoggingEventBus`; trocar por `AddKafkaEventBus()` não toca o domínio.
- **PostgreSQL**: local via Docker (dev) → **Neon** (produção, serverless PostgreSQL).
- **Frontend React + TypeScript** (PWA), seguindo as convenções do vault Micro-CRM (Mobile First, TanStack Query, Tailwind).
- **Deploy em nuvem** (decisão 2026-06-09, revisada 2026-06-15): Render (API via Docker) + Neon (DB) + Vercel (frontend) — objetivo de acessar pelo celular. Render escolhido por blueprint (`render.yaml` = infra como código) e deploy Docker direto; free tier hiberna (~30s de cold start), aceitável p/ uso pessoal.

## Modelo de domínio
- **Account** (módulo Accounts): conta onde o dinheiro mora; guarda saldo de abertura. Saldo atual = abertura + (entradas − saídas).
- **Category** (módulo Transactions): Receita ou Despesa.
- **Transaction** (módulo Transactions): receita/despesa via `CreateIncomeOrExpense`; transferência via `CreateTransferPair` (duas pernas ligadas por `TransferGroupId`, salvas na mesma transação SQL). `Amount` sempre positivo + `Direction` (Inflow/Outflow). `OccurredOn` é `DateOnly`. Soft delete.
- Eventos: `TransactionCreatedDomainEvent` (publicado após o commit).

## Fluxo data-driven (Kafka — Fase 2)
`TransactionCreated` → consumer atualiza saldo agregado por conta (idempotente). Futuro: `BudgetExceeded`, `MonthClosed` (alimenta a previsão).

## Fases e esforço (~10–12h/semana)

| Fase | Entrega | Duração | Aprendizado |
|------|---------|---------|-------------|
| **1 — MVP** ✅ em andamento | Accounts + Transactions, dashboard de saldo, PWA, EF+migrations. Sem Kafka. | 6–8 sem | DDD, EF Core, Minimal API, React+TS PWA |
| **2 — Orçamento + Kafka** | Budgets, recorrências, alertas, import CSV/OFX, relatórios. Kafka entra. | 8–10 sem | Producer/consumer, idempotência, DLQ |
| **3 — Investimentos + Previsão** | Carteira, simulador de juros compostos, previsão 3–12 meses, cenários "e se" | 10–12 sem | CQRS, cálculos financeiros, dashboards |
| **4 — Avançado (opcional)** | ML.NET, Open Finance (Pluggy/Belvo), cotações B3, deploy Azure | 12+ sem | ML, integrações, deploy |

MVP ~2 meses · com previsão ~6–7 meses · tudo ~10–12 meses. A 20h/semana, divide por ~1,7.

## Próximos passos

### Deploy em nuvem — ✅ CONCLUÍDO (2026-06-15, branch `feat/cloud-deploy`)

> Objetivo: acessar o app pelo celular. **App no ar:** https://financeflow-api-fco5.onrender.com
> Decisão de arquitetura: **serviço único** no Render (não dois serviços com Vercel). A API .NET serve **também** o frontend React (build copiado pro `wwwroot`, `UseStaticFiles` + `MapFallbackToFile`). UI e API na **mesma origem** → sem CORS, sem segundo deploy. Stack: **Render** (Docker) + **Neon** (Postgres).

| Entrega | Status |
|---|---|
| Dockerfile multi-stage: stage Node builda o front → `wwwroot` + stage .NET | ✅ |
| `render.yaml` (blueprint, region `ohio`) + `.dockerignore` | ✅ |
| `ApiKeyMiddleware` — exige `X-Api-Key` só em `/api/*` (UI estática livre) | ✅ |
| Connection string aceita formato URI do Neon (`PostgresConnectionString.Normalize`) | ✅ |
| Migrations + seed automáticos no boot (`UseDatabaseStartupAsync`) | ✅ |
| Conta Neon + projeto Render (Blueprint) + env vars | ✅ |
| App live, `/health` 200, `/api/*` protegido, UI servida na raiz | ✅ |

**Env vars no Render (serviço único):**
- `ConnectionStrings__Postgres` = URI do Neon (colada como vem; o app converte)
- `ANTHROPIC_API_KEY` = chave da Anthropic *(hoje `placeholder` → chat IA inativo até pôr chave com crédito)*
- `API_KEY` = gerada pelo Render; usada em runtime (`X-Api-Key`) **e** em build-time (vira `VITE_API_KEY` via build-arg, embutida no bundle do front)

**Pendências pós-deploy (ação Thiago):**
- 🔐 Rotacionar a senha do Neon (passou pelo chat) → atualizar `ConnectionStrings__Postgres`.
- 📱 Instalar o PWA no celular ("Adicionar à tela de início").
- 🤖 Pôr chave Anthropic com crédito pra ativar a aba "IA".
- 🔀 Merge `feat/cloud-deploy` → `main` e reapontar o Render pra `main`.

### Fase 1 — itens pendentes (após deploy)

| Prioridade | Entrega | Status |
|---|---|---|
| 🔴 1 | Saldo atual por conta no dashboard (abertura + entradas − saídas por conta) | pendente |
| 🔴 2 | Editar transação (valor, categoria, descrição) | pendente |
| 🔴 3 | Excluir transação — endpoint de soft delete | pendente |
| 🟡 4 | Endpoint de transferência entre contas (`CreateTransferPair`) | pendente |
| 🟢 5 | Testes unitários do domínio (aprendizado, não urgente) | pendente |

**Adiado/descartado:**
- Autenticação JWT — `ApiKeyMiddleware` é suficiente por ora para uso pessoal.
- Testes de integração dos endpoints — entra quando o domínio estiver mais estável.

## Como trabalhamos (modo tutor)
Ciclos de 1–2 semanas: (1) 1 entrega concreta; (2) implementa, traz travas, eu explico o porquê; (3) perguntas tipo entrevista (por que X, como escala, o que quebra em prod).
