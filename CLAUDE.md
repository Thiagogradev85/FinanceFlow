# CLAUDE.md — FinanceFlow

Guia operacional para a IA e para o dev. Curto de propósito. Detalhes do plano em [docs/PLANNING.md](docs/PLANNING.md).

## O que é
Controle de gastos + previsão financeira. Mobile-first PWA. Stack de aprendizado: .NET 10 + React/TS + Kafka, data-driven. Roda 100% local.

## Sobre o desenvolvedor
Thiago — vindo de Node/React, aprendendo C#/.NET **construindo**. Júnior em .NET; Kafka/microsserviços são novos. Quer entender o **porquê**, não só o como. Responder em pt-BR.

## Arquitetura
Monolito modular + DDD. Um host de API (`FinanceFlow.Api`), módulos isolados em `src/Modules/<Modulo>/{Domain,Application,Infrastructure}`. Um Postgres, um **schema por módulo**.

## Invariantes (não quebrar)
1. **SharedKernel não referencia NuGet externo** — só `System.*`.
2. Módulos se comunicam **apenas por eventos de domínio** (`IEventBus`). Proibida chamada direta entre módulos.
3. Entidades: **construtor de domínio privado** (`private Foo() {}` p/ EF) + **factory methods estáticos** que validam invariantes. Sem setter público.
4. Commands/operações de domínio retornam **`Result`/`Result<T>` com `AppError`** — **nunca** lançam `Exception` para fluxo de negócio. (Exceção: validação de ENTRADA via FluentValidation → 422, centralizada no `ValidationExceptionHandler`.)
5. **Nunca `DateTime.Now`** — usar `DateTime.UtcNow` ou `IClock`.
6. CQRS com **MediatR** (um handler por command/query); validação com **FluentValidation**.
7. **Minimal API** (não MVC controllers).
8. Frontend **Mobile First** (Tailwind `sm:`/`md:`/`lg:`), React + TS, TanStack Query.

## Convenções
- `PascalCase` tipos/métodos; `camelCase` locais/params; `_camelCase` campos privados; `I` em interfaces.
- Um tipo por arquivo (DTOs/records pequenos podem agrupar por feature). Namespaces espelham pastas.
- `UnitOfWork` é **por módulo** (`ITransactionsUnitOfWork`, `IAccountsUnitOfWork`) para o DI saber qual `DbContext` salvar.
- Testes: xUnit, nome `Metodo_Estado_ResultadoEsperado`, AAA com linha em branco entre blocos.

## Comandos
- Rodar tudo: **F5** no VS Code (config `🚀 FinanceFlow (back + front)`) ou `npm run dev`.
- Testes: `npm test` ou `dotnet test FinanceFlow.slnx`.
- Migrations: `dotnet ef migrations add <Nome> --project <Modulo>.Infrastructure --startup-project src/FinanceFlow.Api --context <Modulo>DbContext --output-dir Persistence/Migrations`.
  - ⚠️ Depois de gerar migration, **buildar a solution toda** antes de rodar a API (o `add` builda antes de criar o arquivo; o F5 já builda no preLaunch).
- Kafka (Fase 2): `npm run kafka:up`.

## O que NÃO fazer
- Não pôr lógica de negócio em endpoints.
- Não acessar `DbContext` na Application — só via repositório.
- Não criar dependência de um módulo em outro.
- Não usar MediatR ≥ 13 nem FluentAssertions ≥ 8 (licença comercial); manter MediatR 12.x.

## Estado atual
Fase 1 (MVP) ponta a ponta: Accounts, Transactions, dashboard. `IEventBus` = `LoggingEventBus` (Kafka só na Fase 2). Autenticação ainda não existe — usa `DemoUser` fixo.
