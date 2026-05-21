namespace FinanceFlow.Modules.Transactions.Domain;

public enum TransactionType
{
    Income = 1,
    Expense = 2,
    Transfer = 3
}

/// <summary>
/// Em vez de usar sinal no Amount (-500), separamos "quanto" (Amount, sempre positivo)
/// de "para onde" (Direction). Saldo = soma(Inflow) − soma(Outflow). Impossível esquecer o sinal.
/// </summary>
public enum TransactionDirection
{
    Inflow = 1,   // entrou na conta
    Outflow = 2   // saiu da conta
}
