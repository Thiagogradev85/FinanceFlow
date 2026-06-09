# FinanceFlow — Planejamento

## Visão
App de controle de gastos da vida + previsão de meses futuros + projeção de investimento/poupança. Mobile-first, instalável (PWA) — "desktop app" e mobile com uma base de código só. Projeto de **aprendizado** do stack que se usa na XP (.NET, React, Kafka, data-driven), com Claude Code no papel de tutor.

## Decisões
- **.NET 10** (alinhado às refs TechfinChallenge e Micro-CRM; toolchain local é net10).
- **Monolito modular** (1 host de API, módulos isolados) — não microsserviços. Migração futura é viável porque os módulos já são isolados.
- **Kafka** para o fluxo data-driven, mas só na Fase 2. Na Fase 1 o `IEventBus` (no SharedKernel) usa um `LoggingEventBus`; trocar por `AddKafkaEventBus()` não toca o domínio.
- **PostgreSQL**: local via Docker (dev) → **Neon** (produção, serverless PostgreSQL).
- **Frontend React + TypeScript** (PWA), seguindo as convenções do vault Micro-CRM (Mobile First, TanStack Query, Tailwind).
- **Deploy em nuvem** (decisão 2026-06-09): Railway (API) + Neon (DB) + Vercel (frontend) — objetivo de acessar pelo celular. Railway escolhido sobre Render pelo free tier sem cold start.

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

### Deploy em nuvem — branch `feat/cloud-deploy` (em andamento)

> Objetivo: acessar o app pelo celular. Stack: Railway + Neon + Vercel.

| Prioridade | Entrega | Status |
|---|---|---|
| ✅ | Dockerfile multi-stage .NET 10 | feito |
| ✅ | `railway.toml` + `.dockerignore` | feito |
| ✅ | `appsettings.Production.json` | feito |
| ✅ | CORS dinâmico via `AllowedOrigins` env var | feito |
| ✅ | `ApiKeyMiddleware` (proteção mínima) | feito |
| ✅ | Trocar Copilot → Claude (`ClaudeFinancialAssistant`) | feito |
| ✅ | Frontend `api.ts` com `VITE_API_URL` + `.env.example` | feito |
| ✅ | Assistente IA via chat (`ChatScreen`, `AssistantEndpoints`) | feito |
| 🔴 | Criar conta Neon → copiar connection string | **pendente (Thiago)** |
| 🔴 | Criar projeto Railway → conectar GitHub → setar env vars | **pendente (Thiago)** |
| 🔴 | Rodar migrations contra Neon | **pendente (Thiago)** |
| 🔴 | Criar projeto Vercel → setar `VITE_API_URL` + `VITE_API_KEY` | **pendente (Thiago)** |
| 🔴 | Testar PWA no celular (instalar via "Adicionar à tela de início") | **pendente** |

**Env vars necessárias no Railway:**
- `ConnectionStrings__Postgres` = connection string do Neon
- `ANTHROPIC_API_KEY` = chave da Anthropic
- `AllowedOrigins` = URL do Vercel (ex: `https://financeflow.vercel.app`)
- `API_KEY` = senha forte qualquer

**Env vars necessárias no Vercel:**
- `VITE_API_URL` = URL do Railway (ex: `https://financeflow-api.railway.app`)
- `VITE_API_KEY` = mesma senha do `API_KEY` acima

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
