using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Transactions.Application;

/// <summary>
/// UnitOfWork específico deste módulo. Existe para o container DI saber QUAL DbContext
/// salvar (cada módulo tem o seu). Sem isso, injetar o IUnitOfWork genérico ficaria ambíguo
/// e um handler de Transactions poderia, por acidente, salvar o contexto de Accounts.
/// </summary>
public interface ITransactionsUnitOfWork : IUnitOfWork;
