import { useCategories, useTransactions } from "../lib/hooks";
import { brl, shortDate } from "../lib/format";
import type { TransactionDto } from "../lib/types";

export default function TransactionList({
  onSelect,
  limit,
  year,
  month,
}: {
  onSelect?: (t: TransactionDto) => void;
  limit?: number;
  year?: number;
  month?: number;
}) {
  const { data, isPending, isFetching, isError } = useTransactions(year, month);
  const { data: categories } = useCategories();

  const categoryName = (id: string | null) => categories?.find((c) => c.id === id)?.name ?? "Transferência";
  const items = limit ? data?.slice(0, limit) : data;

  return (
    <div className="space-y-2">
      {isPending && <p className="text-sm text-slate-500">Carregando…</p>}
      {!isPending && isFetching && <p className="text-xs text-slate-500 animate-pulse">Atualizando…</p>}
      {isError && <p className="text-sm text-rose-400">Erro ao carregar transações. Verifique se a API está rodando.</p>}
      {!isPending && !isError && data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma transação ainda.</p>}

      {items?.map((t) => {
        const isIncome = t.direction === 1;
        const isCarryForward = t.isCarryForward === true;
        return (
          <button
            key={isCarryForward ? `carry-${t.occurredOn}` : t.id}
            onClick={isCarryForward ? undefined : () => onSelect?.(t)}
            disabled={!onSelect || isCarryForward}
            className={`flex w-full items-center justify-between rounded-xl p-3 text-left disabled:cursor-default ${
              isCarryForward
                ? "border border-slate-600/40 bg-slate-700/50"
                : "bg-slate-800"
            }`}
          >
            <div>
              <p className={`flex items-center gap-2 text-sm ${isCarryForward ? "italic text-slate-400" : "text-white"}`}>
                <span>{t.description || categoryName(t.categoryId)}</span>
                {isCarryForward && (
                  <span className="rounded-full bg-slate-600/40 px-2 py-0.5 text-[10px] font-medium text-slate-400">
                    ↩ anterior
                  </span>
                )}
                {!isCarryForward && t.installmentCount && (
                  <span className="rounded-full bg-indigo-500/20 px-2 py-0.5 text-[10px] font-medium text-indigo-300">
                    {t.installmentNumber}/{t.installmentCount}
                  </span>
                )}
              </p>
              <p className="text-xs text-slate-500">
                {isCarryForward ? shortDate(t.occurredOn) : `${categoryName(t.categoryId)} · ${shortDate(t.occurredOn)}`}
              </p>
            </div>
            <span className={isIncome ? "font-semibold text-emerald-400" : "font-semibold text-rose-400"}>
              {isIncome ? "+ " : "− "}{brl(t.amount)}
            </span>
          </button>
        );
      })}
    </div>
  );
}
