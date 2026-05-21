# FinanceFlow

App de **controle de gastos e previsão financeira**, mobile-first (PWA instalável), construído como projeto de aprendizado do stack **.NET + React + Kafka, data-driven**.

> Monolito modular em **.NET 10** (DDD por módulos), frontend **React + TypeScript (Vite, PWA)**, **PostgreSQL** e **Kafka** (Fase 2). Roda 100% local.

---

## Como rodar

### Opção 1 — VS Code (F5) ✅ recomendado
Abra a pasta no VS Code e aperte **F5**. A configuração `🚀 FinanceFlow (back + front)`:
1. garante o Docker ligado e sobe o Postgres (`docker compose up -d`);
2. builda a solution;
3. inicia o Vite (abre o navegador em `http://localhost:5173`);
4. roda a API .NET com o **debugger anexado** (`http://localhost:5080`, Swagger em `/swagger`).

> Pré-requisitos: .NET 10 SDK, Node 20+, Docker Desktop, e as extensões recomendadas (VS Code sugere ao abrir: C#, Docker).

### Opção 2 — linha de comando
```bash
npm install            # uma vez (raiz)
npm install --prefix frontend   # uma vez (frontend)
npm run dev            # sobe Docker + API + frontend juntos
```

Outros scripts: `npm run dev:back` (só API), `npm run dev:front` (só web), `npm run kafka:up` (sobe Kafka — Fase 2), `npm test` (testes do back).

No primeiro boot a API **aplica as migrations e semeia dados de exemplo** sozinha (2 contas, 3 categorias, 3 transações).

---

## Endpoints (Fase 1)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/dashboard?year=&month=` | Saldo total + entradas/saídas do mês |
| GET / POST | `/api/accounts` | Listar / criar contas |
| GET / POST | `/api/categories` | Listar / criar categorias |
| GET / POST | `/api/transactions?year=&month=` | Listar / criar transações |
| GET | `/health` | Healthcheck |

Swagger: `http://localhost:5080/swagger`.

---

## Arquitetura — Monolito Modular + DDD

```
src/
├─ FinanceFlow.SharedKernel/      # Entity, AggregateRoot, ValueObject, Result, AppError,
│                                 #   IRepository, IUnitOfWork, IEventBus, IClock  (zero NuGet)
├─ FinanceFlow.Messaging.Kafka/   # IEventBus em Kafka + consumer (Fase 2)
├─ Modules/
│  ├─ Accounts/      Domain · Application · Infrastructure
│  └─ Transactions/  Domain · Application · Infrastructure
└─ FinanceFlow.Api/               # host único: Minimal API, MediatR, Swagger, EF
tests/
└─ FinanceFlow.UnitTests/
frontend/                         # React + TS + Vite (PWA, mobile-first, TanStack Query)
```

**Regras-chave:** módulos conversam **só por eventos de domínio** (nada de chamada direta entre módulos); um banco Postgres, **um schema por módulo** (`accounts`, `transactions`); domínio rico (construtor privado + factory methods + invariantes); commands retornam `Result`/`AppError` (sem exceção pra fluxo de negócio).

Veja [docs/PLANNING.md](docs/PLANNING.md) para o roadmap completo (fases e esforço) e [CLAUDE.md](CLAUDE.md) para as convenções.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | .NET 10, ASP.NET Core Minimal API, EF Core 10, MediatR (CQRS), FluentValidation, Serilog |
| Banco | PostgreSQL 16 (Docker) |
| Mensageria | Kafka (Confluent.Kafka) — Fase 2 |
| Frontend | React 18 + TypeScript + Vite (PWA), Tailwind, TanStack Query |
| Testes | xUnit |

## Status
**Fase 1 (MVP)** funcionando: contas, categorias, transações e dashboard ponta a ponta — ainda **sem Kafka** (o `IEventBus` usa um logger; trocar por Kafka na Fase 2 é uma linha).
