import { useState, type FormEvent, type ReactNode } from "react";
import { X } from "lucide-react";
import { useAccounts, useCategories, useCreateTransaction } from "../lib/hooks";
import { CategoryKind, TransactionType } from "../lib/types";

const inputCls =
  "w-full rounded-xl bg-slate-900 px-3 py-2 text-white outline-none focus:ring-2 focus:ring-emerald-500";

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="mb-3 block">
      <span className="mb-1 block text-xs text-slate-400">{label}</span>
      {children}
    </label>
  );
}

export default function AddTransactionForm({ onClose }: { onClose: () => void }) {
  const { data: accounts } = useAccounts();
  const { data: categories } = useCategories();
  const create = useCreateTransaction();

  const [type, setType] = useState<number>(TransactionType.Expense);
  const [accountId, setAccountId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [occurredOn, setOccurredOn] = useState(new Date().toISOString().slice(0, 10));

  const filteredCategories = categories?.filter((c) =>
    type === TransactionType.Income ? c.kind === CategoryKind.Income : c.kind === CategoryKind.Expense,
  );

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    await create.mutateAsync({
      accountId: accountId || accounts?.[0]?.id || "",
      categoryId: categoryId || filteredCategories?.[0]?.id || "",
      type,
      amount: Number(amount),
      occurredOn,
      description,
    });
    onClose();
  };

  return (
    <div className="fixed inset-0 z-10 flex items-end bg-black/50" onClick={onClose}>
      <form onClick={(e) => e.stopPropagation()} onSubmit={submit} className="w-full rounded-t-3xl bg-slate-800 p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-semibold text-white">Nova transação</h3>
          <button type="button" onClick={onClose} className="text-slate-400">
            <X />
          </button>
        </div>

        <div className="mb-3 grid grid-cols-2 gap-2">
          <button
            type="button"
            onClick={() => setType(TransactionType.Expense)}
            className={`rounded-xl py-2 text-sm font-medium ${type === TransactionType.Expense ? "bg-rose-500 text-white" : "bg-slate-900 text-slate-400"}`}
          >
            Despesa
          </button>
          <button
            type="button"
            onClick={() => setType(TransactionType.Income)}
            className={`rounded-xl py-2 text-sm font-medium ${type === TransactionType.Income ? "bg-emerald-500 text-slate-900" : "bg-slate-900 text-slate-400"}`}
          >
            Receita
          </button>
        </div>

        <Field label="Valor">
          <input type="number" step="0.01" min="0" required value={amount} onChange={(e) => setAmount(e.target.value)} className={inputCls} placeholder="0,00" />
        </Field>
        <Field label="Conta">
          <select value={accountId} onChange={(e) => setAccountId(e.target.value)} className={inputCls}>
            {accounts?.map((a) => (
              <option key={a.id} value={a.id}>{a.name}</option>
            ))}
          </select>
        </Field>
        <Field label="Categoria">
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className={inputCls}>
            {filteredCategories?.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </Field>
        <Field label="Descrição">
          <input value={description} onChange={(e) => setDescription(e.target.value)} className={inputCls} placeholder="Ex.: Mercado" />
        </Field>
        <Field label="Data">
          <input type="date" value={occurredOn} onChange={(e) => setOccurredOn(e.target.value)} className={inputCls} />
        </Field>

        {create.isError && <p className="mb-2 text-sm text-rose-400">Erro ao salvar. Confira os campos.</p>}

        <button type="submit" disabled={create.isPending} className="mt-2 w-full rounded-xl bg-emerald-500 py-3 font-semibold text-slate-900 disabled:opacity-60">
          {create.isPending ? "Salvando…" : "Salvar"}
        </button>
      </form>
    </div>
  );
}
