# FinanceFlow — Planejamento

## Visão
App de controle de gastos da vida + previsão de meses futuros + projeção de investimento/poupança. Mobile-first, instalável (PWA) — "desktop app" e mobile com uma base de código só. Projeto de **aprendizado** do stack que se usa na XP (.NET, React, Kafka, data-driven), com Claude Code no papel de tutor.

## Decisões
- **.NET 10** (alinhado às refs TechfinChallenge e Micro-CRM; toolchain local é net10).
- **Monolito modular** (1 host de API, módulos isolados) — não microsserviços. Migração futura é viável porque os módulos já são isolados.
- **Kafka** para o fluxo data-driven, mas só na Fase 2. Na Fase 1 o `IEventBus` (no SharedKernel) usa um `LoggingEventBus`; trocar por `AddKafkaEventBus()` não toca o domínio.
- **PostgreSQL no Docker**, 100% local (volume no disco). Um banco, um schema por módulo.
- **Frontend React + TypeScript** (PWA), seguindo as convenções do vault Micro-CRM (Mobile First, TanStack Query, Tailwind).

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

## Próximos passos (Fase 1 → 1.5)
1. Autenticação (JWT + refresh) substituindo o `DemoUser`.
2. Endpoint de transferência (`CreateTransferPair`) exposto na API.
3. Saldo atual por conta (hoje o dashboard usa abertura + líquido global).
4. Editar/excluir transação (soft delete já existe no domínio).
5. Testes de integração dos endpoints.

## Como trabalhamos (modo tutor)
Ciclos de 1–2 semanas: (1) 1 entrega concreta; (2) implementa, traz travas, eu explico o porquê; (3) perguntas tipo entrevista (por que X, como escala, o que quebra em prod).
