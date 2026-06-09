import type { ReactNode } from "react";
import type { UseQueryResult } from "@tanstack/react-query";
import { TrendingDown, TrendingUp } from "lucide-react";
import type { DashboardDto } from "../lib/types";
import { brl } from "../lib/format";

function Card({ children }: { children: ReactNode }) {
  return <section className="mb-6 rounded-2xl bg-slate-800 p-5 text-slate-400">{children}</section>;
}

export default function BalanceCard({ query }: { query: UseQueryResult<DashboardDto> }) {
  if (query.isPending) return <Card>Carregando resumo…</Card>;
  if (query.isError || !query.data) return <Card>Não foi possível carregar o resumo.</Card>;

  const d = query.data;

  return (
    <section className="mb-6 rounded-2xl bg-slate-800 p-5">
      <p className="text-sm text-slate-400">Saldo total</p>
      <p className="mb-4 text-3xl font-bold text-white">{brl(d.totalBalance)}</p>

      <div className="grid grid-cols-2 gap-3">
        <div className="rounded-xl bg-slate-900/60 p-3">
          <div className="flex items-center gap-1 text-emerald-400">
            <TrendingUp size={16} />
            <span className="text-xs">Entradas (mês)</span>
          </div>
          <p className="mt-1 font-semibold text-white">{brl(d.monthIncome)}</p>
        </div>
        <div className="rounded-xl bg-slate-900/60 p-3">
          <div className="flex items-center gap-1 text-rose-400">
            <TrendingDown size={16} />
            <span className="text-xs">Saídas (mês)</span>
          </div>
          <p className="mt-1 font-semibold text-white">{brl(d.monthExpense)}</p>
        </div>
      </div>
    </section>
  );
}
