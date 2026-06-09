// Espelham os DTOs da API (.NET serializa em camelCase por padrão).

export interface AccountDto {
  id: string;
  name: string;
  type: number;
  currency: string;
  openingBalance: number;
}

export interface CategoryDto {
  id: string;
  name: string;
  kind: number; // 1 = Receita, 2 = Despesa
  color: string;
  icon: string;
}

export interface TransactionDto {
  id: string;
  accountId: string;
  categoryId: string | null;
  type: number; // 1 = Receita, 2 = Despesa, 3 = Transferência
  direction: number; // 1 = Entrada, 2 = Saída
  amount: number;
  currency: string;
  occurredOn: string; // "2026-05-21"
  description: string;
}

export interface DashboardDto {
  year: number;
  month: number;
  totalBalance: number;
  monthIncome: number;
  monthExpense: number;
  monthNet: number;
}

export const TransactionType = { Income: 1, Expense: 2, Transfer: 3 } as const;
export const CategoryKind = { Income: 1, Expense: 2 } as const;

export interface TransactionProposal {
  accountId: string;
  accountName: string;
  categoryId: string;
  categoryName: string;
  type: number; // 1=Income, 2=Expense
  amount: number;
  occurredOn: string; // "YYYY-MM-DD"
  description: string;
}
