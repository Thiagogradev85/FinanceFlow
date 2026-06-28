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
        return (
          <button
            key={t.id}
            onClick={() => onSelect?.(t)}
            disabled={!onSelect}
            className="flex w-full items-center justify-between rounded-xl bg-slate-800 p-3 text-left disabled:cursor-default"
          >
            <div>
              <p className="flex items-center gap-2 text-sm text-white">
                <span>{t.description || categoryName(t.categoryId)}</span>
                {t.installmentCount && (
                  <span className="rounded-full bg-indigo-500/20 px-2 py-0.5 text-[10px] font-medium text-indigo-300">
                    {t.installmentNumber}/{t.installmentCount}
                  </span>
                )}
              </p>
              <p className="text-xs text-slate-500">{categoryName(t.categoryId)} · {shortDate(t.occurredOn)}</p>
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
