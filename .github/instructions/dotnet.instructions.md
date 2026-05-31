---
applyTo: "**/*.cs,**/*.csproj,src/**"
---

# Regras para código .NET/C#

- Use `DateTime.UtcNow` ou injete `IClock` — nunca `DateTime.Now`.
- Retorne `Result<T>` ou `Result` com `AppError` para fluxo de negócio — nunca lance `Exception`.
- Entidades de domínio: construtor `private` para EF + factory method estático que valida invariantes.
- Um handler MediatR por command/query. Validators FluentValidation em arquivo separado.
- `UnitOfWork` é por módulo — nunca use uma interface `IUnitOfWork` genérica em projetos com múltiplos DbContexts.
- Proibido MediatR >= 13 e FluentAssertions >= 8 (licença comercial).
- Testes: xUnit, nome `Metodo_Estado_ResultadoEsperado`, AAA com linha em branco entre blocos.
