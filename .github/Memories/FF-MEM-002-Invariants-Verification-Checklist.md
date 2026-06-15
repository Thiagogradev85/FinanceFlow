# Memory: FinanceFlow — Checklist de Verificação de Invariantes

## Metadata

- PatternId: FF-MEM-002
- PatternVersion: 1
- Status: active
- Supersedes: none
- CreatedAt: 2026-06-15
- LastValidatedAt: 2026-06-15
- ValidationEvidence: Análise arquitetural Fase 1 — invariantes declaradas no CLAUDE.md

## Source Context

- Triggering task: Revisão arquitetural completa Fase 1
- Scope/system: FinanceFlow — camadas Domain e Application de todos os módulos
- Date/time: 2026-06-15

## Memory

- Key fact: 8 invariantes críticas declaradas no CLAUDE.md. As mais propensas a **violação silenciosa** em MVP rápido são:
  - **#3** — setters públicos em entidades (EF Core tenta setar via reflection, tentação de adicionar `{ get; set; }`)
  - **#4** — `throw new Exception` em vez de `Result<T>` / `AppError` em handlers de Application layer
  - **#5** — `DateTime.Now` em vez de `DateTime.UtcNow` ou `IClock` injetado
- Why it matters: Violations nestas 3 invariantes são **silenciosas em tempo de compilação** mas quebram testabilidade e semântica de erro. EF Core funciona com construtor privado + mapeamento explícito via `.HasField` ou `.Property(x => x.Prop)`.

## Applicability

- When to reuse: Code review, PRs, ao implementar novos aggregates ou command handlers
- Preconditions/limitations: Aplicável a todas as camadas Domain e Application de qualquer módulo

## Actionable Guidance

- **Grep periódico** para detectar violações:
  ```bash
  # Invariante #5 — DateTime.Now
  grep -rn "DateTime\.Now" src/Modules/ src/FinanceFlow.Api/Common/

  # Invariante #3 — setters públicos em entidades (candidatos a violação)
  grep -rn "public.*{ get; set; }" src/Modules/*/Domain/

  # Invariante #4 — throw em Application layer
  grep -rn "throw new.*Exception" src/Modules/*/Application/
  ```
- **EF Core com construtor privado:** usar `.HasField("_campo")` no `OnModelCreating` e inicializar coleções via `= new List<T>()` no campo privado.
- **IClock:** injetar via construtor em factories estáticas usando parâmetro; em entities usar `DateTime.UtcNow` apenas se `IClock` não for viável no static factory, e então testar com clock mockado via handler.
- Related files: `src/FinanceFlow.SharedKernel/Abstractions/IClock.cs`, `src/FinanceFlow.SharedKernel/Results/Result.cs`
