import { useCategories, useTransactions } from "../lib/hooks";
import { brl, shortDate } from "../lib/format";

export default function TransactionList() {
  const { data, isLoading } = useTransactions();
  const { data: categories } = useCategories();

  const categoryName = (id: string | null) =>
    categories?.find((c) => c.id === id)?.name ?? "Transferência";

  return (
    <section className="mb-6">
      <h2 className="mb-2 text-sm font-semibold text-slate-300">Transações do mês</h2>
      {isLoading && <p className="text-sm text-slate-500">Carregando…</p>}
      {data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma transação ainda.</p>}

      <div className="space-y-2">
        {data?.map((t) => {
          const isIncome = t.direction === 1; // 1 = Entrada
          return (
            <div key={t.id} className="flex items-center justify-between rounded-xl bg-slate-800 p-3">
              <div>
                <p className="text-sm text-white">{t.description || categoryName(t.categoryId)}</p>
                <p className="text-xs text-slate-500">
                  {categoryName(t.categoryId)} · {shortDate(t.occurredOn)}
                </p>
              </div>
              <span className={isIncome ? "font-semibold text-emerald-400" : "font-semibold text-rose-400"}>
                {isIncome ? "+ " : "− "}
                {brl(t.amount)}
              </span>
            </div>
          );
        })}
      </div>
    </section>
  );
}
