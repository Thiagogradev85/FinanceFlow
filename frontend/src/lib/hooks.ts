import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import api from "./api";
import type { AccountDto, CategoryDto, DashboardDto, TransactionDto } from "./types";

export const useDashboard = () =>
  useQuery({
    queryKey: ["dashboard"],
    queryFn: async () => (await api.get<DashboardDto>("/dashboard")).data,
  });

export const useAccounts = () =>
  useQuery({
    queryKey: ["accounts"],
    queryFn: async () => (await api.get<AccountDto[]>("/accounts")).data,
  });

export const useCategories = () =>
  useQuery({
    queryKey: ["categories"],
    queryFn: async () => (await api.get<CategoryDto[]>("/categories")).data,
  });

export const useTransactions = () =>
  useQuery({
    queryKey: ["transactions"],
    queryFn: async () => (await api.get<TransactionDto[]>("/transactions")).data,
  });

export interface CreateTransactionInput {
  accountId: string;
  categoryId: string;
  type: number;
  amount: number;
  occurredOn: string;
  description: string;
}

export const useCreateTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (input: CreateTransactionInput) =>
      (await api.post<TransactionDto>("/transactions", { currency: "BRL", ...input })).data,
    // Após criar, o TanStack Query revalida dashboard e lista — UI sempre fresca.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });
};
