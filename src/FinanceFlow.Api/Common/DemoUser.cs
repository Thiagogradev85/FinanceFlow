namespace FinanceFlow.Api.Common;

/// <summary>
/// Fase 1 ainda não tem autenticação. Usamos um usuário fixo para tudo funcionar ponta a ponta.
/// Na Fase 1.5/2 isto vira o usuário real extraído do JWT (claim "sub").
/// </summary>
public static class DemoUser
{
    public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
