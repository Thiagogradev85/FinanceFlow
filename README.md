# FinanceFlow

App de **controle de gastos e previsão financeira**, mobile-first (PWA instalável), construído como projeto de aprendizado do stack **.NET + React + Kafka, data-driven**.

> Monolito modular em **.NET 10** (DDD por módulos), frontend **React + TypeScript (Vite, PWA)**, **PostgreSQL** e **Kafka** (Fase 2). Dev local via Docker; produção em nuvem (serviço único no Render + Neon).

---

## Como rodar

> **Ambiente canônico:** Ubuntu **WSL2** com o clone em **ext4 nativo** (`/home/thiag/projects/FinanceFlow`) e **Docker Engine** (sem Docker Desktop). Abra o VS Code em modo WSL apontando pra essa pasta. Detalhes na Opção 3.

### Opção 1 — VS Code (F5) ✅ recomendado
Abra a pasta no VS Code e aperte **F5**. A configuração `🚀 FinanceFlow (back + front)`:
1. garante o Docker ligado e sobe o Postgres (`docker compose up -d`);
2. builda a solution;
3. inicia o Vite (abre o navegador em `http://localhost:5173`);
4. roda a API .NET com o **debugger anexado** (`http://localhost:5080`, Swagger em `/swagger`).

> Pré-requisitos (no WSL2): .NET 10 SDK, Node 20+, **Docker Engine** (não Docker Desktop — ver Opção 3), e as extensões recomendadas (VS Code sugere ao abrir: C#, Docker). Abra o VS Code em **modo WSL**.

### Opção 2 — linha de comando
```bash
npm install            # uma vez (raiz)
npm install --prefix frontend   # uma vez (frontend)
npm run dev            # sobe Docker + API + frontend juntos
```

Outros scripts: `npm run dev:back` (só API), `npm run dev:front` (só web), `npm run kafka:up` (sobe Kafka — Fase 2), `npm test` (testes do back).

No primeiro boot a API **aplica as migrations e semeia dados de exemplo** sozinha (2 contas, 3 categorias, 3 transações).

### Opção 3 — WSL2 (Linux nativo, sem Docker Desktop) ⭐ ambiente canônico
Docker Engine + .NET SDK + Node todos nativos no Ubuntu WSL2, com o clone em **ext4 real** (não em `/mnt/c`). É o ambiente oficial do projeto — alinhado com a XP e ~10x mais rápido em I/O que rodar a partir do disco do Windows.

Pré-requisitos no **Ubuntu WSL2 (24.04+)**:
- **Docker Engine** + plugins via repo oficial: `docker-ce`, `docker-ce-cli`, `containerd.io`, `docker-buildx-plugin`, `docker-compose-plugin`
- **.NET 10 SDK** (`dotnet-sdk-10.0`) via `packages.microsoft.com`
- **Node 20+** (via [fnm](https://github.com/Schniz/fnm) ou nvm)
- **`gh` CLI** autenticado (`sudo apt install gh && gh auth login`)

Clone em `~/projects/` (não em `/mnt/c/...`) — ext4 nativo é ~10x mais rápido em I/O:

```bash
gh repo clone Thiagogradev85/FinanceFlow ~/projects/FinanceFlow
cd ~/projects/FinanceFlow
npm install && npm install --prefix frontend
npm run dev
```

A `global.json` usa `"rollForward": "latestFeature"` — qualquer SDK .NET 10.0.X serve. As portas `:5173` (Vite) e `:5080` (API) ficam acessíveis no navegador Windows automaticamente via WSL2 port-forwarding.

---

## Comandos úteis

### Subir / parar a stack
```bash
docker compose up -d           # sobe Postgres + pgAdmin
docker compose stop            # pausa (mantém containers e dados)
docker compose start           # retoma após stop
docker compose down            # remove containers (volume preservado)
docker compose down -v         # ⚠️ remove tudo, inclusive volume (apaga o DB)
docker compose ps              # status dos serviços
docker compose logs -f         # logs ao vivo (Ctrl+C sai)
```

### Rodar o app
```bash
npm run dev                    # tudo: ensure Docker + API + Vite (concurrently)
npm run dev:back               # só a API
npm run dev:front              # só o front (Vite)
npm test                       # testes do back (xUnit)
```

> Pra parar o `npm run dev`: `Ctrl+C` no terminal — derruba API e Vite juntos.

### Build / restore (.NET)
```bash
dotnet build FinanceFlow.slnx
dotnet test FinanceFlow.slnx --no-build
dotnet restore
```

### Migrations EF Core
```bash
dotnet ef migrations add <Nome> \
  --project src/Modules/<Modulo>/FinanceFlow.Modules.<Modulo>.Infrastructure \
  --startup-project src/FinanceFlow.Api \
  --context <Modulo>DbContext \
  --output-dir Persistence/Migrations
```
> ⚠️ Após criar a migration, **rebuild a solution toda** antes de rodar a API — o `add` builda antes de gravar o arquivo, e o F5/`npm run dev` espera o build atualizado.

### Kafka (Fase 2)
```bash
npm run kafka:up               # sobe Kafka + Kafka UI
npm run kafka:down             # para
```

### Endereços locais
| Serviço | URL | Notas |
|---|---|---|
| Frontend (PWA) | http://localhost:5173 | Vite dev server |
| API | http://localhost:5080 | Minimal API .NET |
| Swagger | http://localhost:5080/swagger | docs da API |
| pgAdmin | http://localhost:5050 | login: `admin@example.com` / `admin123` |
| Kafka UI (Fase 2) | http://localhost:8080 | só com `npm run kafka:up` |

### Gotchas do WSL2
```bash
# git push pendurado por causa do Git Credential Manager do Windows?
# Roda 1x por máquina e o gh assume o lugar do helper:
gh auth setup-git

# Docker daemon parou?
sudo systemctl start docker
systemctl is-active docker     # deve responder "active"

# Conferir que o docker em uso é o nativo do Linux (e não o stub do Docker Desktop):
which docker                   # esperado: /usr/bin/docker
```

---

## Deploy em nuvem (Render + Neon)

Stack de produção: **Render** (serviço único via Docker, blueprint `render.yaml`) · **Neon** (Postgres serverless). **Uma URL só** serve a API **e** o frontend: o `Dockerfile` builda o React e copia o `dist/` pro `wwwroot`, e a API .NET serve os estáticos (`UseStaticFiles` + `MapFallbackToFile`). Como UI e API ficam na **mesma origem**, não há CORS nem segundo deploy.

**Variáveis de ambiente (no Render):**
| Variável | Valor |
|---|---|
| `ConnectionStrings__Postgres` | connection string do Neon (ver nota abaixo) |
| `ANTHROPIC_API_KEY` | chave da Anthropic (assistente IA usa `ClaudeFinancialAssistant` em prod) |
| `API_KEY` | senha forte. Em runtime o `ApiKeyMiddleware` exige ela em `X-Api-Key` nas rotas `/api/*`; em build-time o Render a passa como build-arg e o Dockerfile a injeta em `VITE_API_KEY` (o front manda no header). |

> ⚠️ O `VITE_API_KEY` fica embutido no bundle JS (baixável pelo navegador) — é **proteção mínima** contra bots, não contra um humano. Autenticação real (JWT) é a Fase 1.5.

> **Connection string — aceita os dois formatos.** Provedores de nuvem (Neon, Heroku…) entregam a string no formato **URI** (`postgresql://user:pass@host/db?sslmode=require`), mas o Npgsql só entende o formato **key-value** (`Host=...;Database=...;Username=...`). O [`PostgresConnectionString.Normalize`](src/FinanceFlow.Api/Common/PostgresConnectionString.cs) detecta a URI e converte automaticamente no boot — então **cole a string do Neon como ela vem**, sem tradução manual. Strings key-value (dev local) passam intactas.

---

## Endpoints (Fase 1)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/dashboard?year=&month=` | Saldo total + entradas/saídas do mês |
| GET / POST / PUT / DELETE | `/api/accounts` · `/api/accounts/{id}` | CRUD de contas |
| GET / POST / PUT / DELETE | `/api/categories` · `/api/categories/{id}` | CRUD de categorias (receita/despesa) |
| GET / POST / PUT / DELETE | `/api/transactions?year=&month=` · `/api/transactions/{id}` | CRUD de transações |
| GET | `/health` | Healthcheck |

> No app a navegação é por **abas** embaixo: Início (saldo + últimas), Transações (toque pra editar), Categorias e Contas. Editar/excluir usa **soft delete** (arquiva, preserva histórico).

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

## Agentes de IA

O projeto tem agentes configurados para **Claude Code** e **GitHub Copilot** no VS Code.

### Claude Code — `.claude/agents/`
| Agente | Como acionar | Papel |
|--------|-------------|-------|
| `guardiao-financeflow` | mencionado automaticamente | Revisa convenções DDD/SharedKernel/Result — somente leitura |
| `arquiteto` | "usa o arquiteto" / "planeja com o arquiteto" | Plano + trade-offs antes de escrever código |
| `revisor-pr` | "revisa o PR" / "passa o revisor" | Code review: segurança, legibilidade, testes |

### GitHub Copilot — `.github/agents/`
| Agente | Como acionar | Papel |
|--------|-------------|-------|
| `@tutor-senior-xp` | Copilot Chat no VS Code | Tutor sênior XP Inc. — explica conceitos, checkpoint obrigatório em hands-on |
| `@guardiao` | Copilot Chat no VS Code | Revisor de convenções genérico |
| `@arquiteto` | Copilot Chat no VS Code | Consultor de arquitetura |
| `@revisor-pr` | Copilot Chat no VS Code | Revisor de PR |

> Instruções gerais para o Copilot em `.github/copilot-instructions.md`. Instruções automáticas por tipo de arquivo em `.github/instructions/`.

---

## Status
**Fase 1 (MVP)** funcionando: contas, categorias, transações e dashboard ponta a ponta — ainda **sem Kafka** (o `IEventBus` usa um logger; trocar por Kafka na Fase 2 é uma linha).
