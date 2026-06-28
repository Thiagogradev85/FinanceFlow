# Compras parceladas + navegação por mês

> Feature entregue em 2026-06-22. Documenta o parcelamento, o saldo realizado×agendado, a navegação por mês na Home e a listagem de transações de todos os meses.

## O problema

Controlar o que já está **comprometido** em parcelas dos próximos meses, sem que essas parcelas futuras distorçam o saldo de hoje.

## Como funciona

### Modelo — N despesas mensais materializadas
Uma compra parcelada vira **N transações reais** (uma por mês), ligadas por um `InstallmentGroupId` — mesmo padrão da transferência (`CreateTransferPair`).

- Factory de domínio: `Transaction.CreateInstallmentPurchase(...)` → `Result<IReadOnlyList<Transaction>>`.
- Campos novos em `Transaction`: `InstallmentGroupId` (Guid?), `InstallmentNumber` (1..N), `InstallmentCount` (N).
- Você informa o **valor da parcela × número de vezes** (não o total) → sem arredondamento; cada mês recebe exatamente o mesmo valor.
- Datas: a 1ª parcela em `firstOccurredOn`; as seguintes em `firstOccurredOn.AddMonths(i)` (o `AddMonths` corrige fim de mês: 31/jan → 28/fev).
- Parcelamento é sempre **despesa** (a categoria precisa ser de despesa).
- As N parcelas são salvas na **mesma transação SQL** (UnitOfWork) — ou todas, ou nenhuma.

Por que materializar em vez de um "plano" projetado: o app já lista e soma **mês a mês por `OccurredOn`**. Gravando as parcelas, cada mês futuro "enxerga" a sua sozinho — sem reescrever nenhuma query. É o modelo mental da fatura de cartão.

### Saldo realizado × agendado
- **Saldo de hoje (realizado):** só conta transações com `OccurredOn <= hoje` → parcelas futuras **não derrubam** o saldo atual.
- **Saldo previsto (navegação por mês):** ao escolher um mês, o saldo mostrado é o **previsto até o fim daquele mês** (`GetAllTimeNetAsync(asOf = último dia do mês)`). Avançar meses desconta as parcelas futuras; voltar mostra como o saldo estava.

### Comprometido (próximos meses)
`GET /api/transactions/commitments?months=N` soma o total parcelado já agendado, mês a mês, a partir do próximo mês. Aparece num card na Home.

### Navegação por mês na Home
Cabeçalho `‹ Mês Ano ›` (componente `MonthSelector`): as setas mudam o mês de referência e atualizam saldo previsto, entradas/saídas do mês e a lista de transações daquele mês.

### Aba Transações — todas, de qualquer mês
`GET /api/transactions` **sem** `year/month` retorna as últimas N (200) transações de **todos os meses** (`ListRecentByUserAsync`), pra ver e **corrigir** lançamentos antigos. Com `year`+`month`, filtra o mês (usado pela Home).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/transactions/installment` | Cria a compra parcelada (N despesas mensais) |
| GET | `/api/transactions/commitments?months=` | Total parcelado comprometido nos próximos meses |
| GET | `/api/transactions?year=&month=` | Sem mês → últimas de todos os meses; com mês → filtra |
| GET | `/api/dashboard?year=&month=` | Saldo previsto até o fim do mês + entradas/saídas do mês |

## Onde está no código

- Domínio: `Transaction.cs` (factory `CreateInstallmentPurchase`), testes em `tests/FinanceFlow.UnitTests/TransactionTests.cs` (9 novos).
- Application: `Transactions/CreateInstallmentPurchase.cs`, `Transactions/GetUpcomingCommitments.cs`, `Transactions/ListTransactions.cs`, `Transactions/GetTransactionsSummary.cs`.
- Infra: `TransactionRepository.cs` (`GetAllTimeNetAsync(asOf)`, `ListRecentByUserAsync`, `GetMonthlyCommittedInstallmentsAsync`), migration `AddInstallments`.
- API: `Endpoints/TransactionsEndpoints.cs`.
- Front: `components/MonthSelector.tsx`, `components/CommitmentsCard.tsx`, `components/TransactionForm.tsx` (toggle "Compra parcelada?"), `components/TransactionList.tsx` (selo `1/12`), `App.tsx`.

## Pendência relacionada
- **Conta principal escolhível** (flag `IsPrimary`): decidida, ainda não implementada. Hoje a "principal" é só a 1ª conta por ordem de nome.
