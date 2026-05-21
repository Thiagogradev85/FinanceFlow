import { useCategories, useTransactions } from "../lib/hooks";
import { brl, shortDate } from "../lib/format";
import type { TransactionDto } from "../lib/types";

export default function TransactionList({
  onSelect,
  limit,
}: {
  onSelect?: (t: TransactionDto) => void;
  limit?: number;
}) {
  const { data, isLoading } = useTransactions();
  const { data: categories } = useCategories();

  const categoryName = (id: string | null) => categories?.find((c) => c.id === id)?.name ?? "Transferência";
  const items = limit ? data?.slice(0, limit) : data;

  return (
    <div className="space-y-2">
      {isLoading && <p className="text-sm text-slate-500">Carregando…</p>}
      {data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma transação ainda.</p>}

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
              <p className="text-sm text-white">{t.description || categoryName(t.categoryId)}</p>
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
