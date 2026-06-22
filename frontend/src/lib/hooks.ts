import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import api from "./api";
import type { AccountDto, CategoryDto, DashboardDto, TransactionDto, UpcomingCommitmentsDto } from "./types";

// ───────────────────────── Queries ─────────────────────────
const ALWAYS = { refetchOnMount: "always" as const };

export const useDashboard = (year?: number, month?: number) =>
  useQuery({
    queryKey: ["dashboard", year ?? null, month ?? null],
    queryFn: async () => (await api.get<DashboardDto>("/dashboard", { params: { year, month } })).data,
    ...ALWAYS,
  });

export const useAccounts = () =>
  useQuery({ queryKey: ["accounts"], queryFn: async () => (await api.get<AccountDto[]>("/accounts")).data, ...ALWAYS });

export const useCategories = () =>
  useQuery({ queryKey: ["categories"], queryFn: async () => (await api.get<CategoryDto[]>("/categories")).data, ...ALWAYS });

export const useTransactions = (year?: number, month?: number) =>
  useQuery({
    queryKey: ["transactions", year ?? null, month ?? null],
    queryFn: async () => (await api.get<TransactionDto[]>("/transactions", { params: { year, month } })).data,
    ...ALWAYS,
  });

export const useCommitments = (months = 6) =>
  useQuery({
    queryKey: ["commitments", months],
    queryFn: async () => (await api.get<UpcomingCommitmentsDto>(`/transactions/commitments?months=${months}`)).data,
    ...ALWAYS,
  });

// ─────────────────────── Transações ────────────────────────
export interface TransactionInput {
  accountId: string;
  categoryId: string;
  type: number;
  amount: number;
  occurredOn: string;
  description: string;
}

export const useCreateTransaction = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: TransactionInput) =>
      (await api.post<TransactionDto>("/transactions", { currency: "BRL", ...input })).data,
    onSuccess: () => invalidate(qc, ["transactions", "dashboard", "commitments"]),
  });
};

export const useUpdateTransaction = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...input }: TransactionInput & { id: string }) =>
      (await api.put<TransactionDto>(`/transactions/${id}`, { currency: "BRL", ...input })).data,
    onSuccess: () => invalidate(qc, ["transactions", "dashboard", "commitments"]),
  });
};

export const useDeleteTransaction = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => api.delete(`/transactions/${id}`),
    onSuccess: () => invalidate(qc, ["transactions", "dashboard", "commitments"]),
  });
};

export interface InstallmentInput {
  accountId: string;
  categoryId: string;
  installmentAmount: number;
  installmentCount: number;
  firstOccurredOn: string;
  description: string;
}

export const useCreateInstallment = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: InstallmentInput) =>
      (await api.post<TransactionDto[]>("/transactions/installment", { currency: "BRL", ...input })).data,
    onSuccess: () => invalidate(qc, ["transactions", "dashboard", "commitments"]),
  });
};

// ─────────────────────── Categorias ────────────────────────
export interface CreateCategoryInput { name: string; kind: number; color: string; icon: string }
export interface UpdateCategoryInput { id: string; name: string; color: string; icon: string }

export const useCreateCategory = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: CreateCategoryInput) => (await api.post<CategoryDto>("/categories", input)).data,
    onSuccess: () => invalidate(qc, ["categories", "transactions"]),
  });
};

export const useUpdateCategory = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...input }: UpdateCategoryInput) =>
      (await api.put<CategoryDto>(`/categories/${id}`, input)).data,
    onSuccess: () => invalidate(qc, ["categories", "transactions"]),
  });
};

export const useDeleteCategory = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => api.delete(`/categories/${id}`),
    onSuccess: () => invalidate(qc, ["categories", "transactions"]),
  });
};

// ───────────────────────── Contas ──────────────────────────
export interface CreateAccountInput { name: string; type: number; currency: string; openingBalance: number }
export interface UpdateAccountInput { id: string; name: string; openingBalance: number }

export const useCreateAccount = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: CreateAccountInput) => (await api.post<AccountDto>("/accounts", input)).data,
    onSuccess: () => invalidate(qc, ["accounts", "dashboard"]),
  });
};

export const useUpdateAccount = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...input }: UpdateAccountInput) =>
      (await api.put<AccountDto>(`/accounts/${id}`, input)).data,
    onSuccess: () => invalidate(qc, ["accounts", "dashboard"]),
  });
};

export const useDeleteAccount = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => api.delete(`/accounts/${id}`),
    onSuccess: () => invalidate(qc, ["accounts", "dashboard"]),
  });
};

function invalidate(qc: ReturnType<typeof useQueryClient>, keys: string[]) {
  keys.forEach((key) => qc.invalidateQueries({ queryKey: [key] }));
}
