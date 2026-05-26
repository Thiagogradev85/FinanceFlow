import { useState, type FormEvent } from "react";
import { Trash2 } from "lucide-react";
import {
  useAccounts,
  useCategories,
  useCreateTransaction,
  useDeleteTransaction,
  useUpdateTransaction,
} from "../lib/hooks";
import { CategoryKind, TransactionType, type TransactionDto } from "../lib/types";
import { Field, Sheet, inputCls } from "./ui";
import { formatBrlInput, parseBrlAmount } from "../lib/format";

export default function TransactionForm({
  editing,
  onClose,
}: {
  editing: TransactionDto | null;
  onClose: () => void;
}) {
  const { data: accounts } = useAccounts();
  const { data: categories } = useCategories();
  const create = useCreateTransaction();
  const update = useUpdateTransaction();
  const remove = useDeleteTransaction();

  const isEdit = editing !== null;

  const [type, setType] = useState<number>(editing?.type ?? TransactionType.Expense);
  const [accountId, setAccountId] = useState(editing?.accountId ?? "");
  const [categoryId, setCategoryId] = useState(editing?.categoryId ?? "");
  const [amount, setAmount] = useState(editing ? formatBrlInput(editing.amount) : "");
  const [description, setDescription] = useState(editing?.description ?? "");
  const [occurredOn, setOccurredOn] = useState(editing?.occurredOn ?? new Date().toISOString().slice(0, 10));

  const filteredCategories = categories?.filter((c) =>
    type === TransactionType.Income ? c.kind === CategoryKind.Income : c.kind === CategoryKind.Expense,
  );

  const busy = create.isPending || update.isPending || remove.isPending;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    const payload = {
      accountId: accountId || accounts?.[0]?.id || "",
      categoryId: categoryId || filteredCategories?.[0]?.id || "",
      type,
      amount: parseBrlAmount(amount),
      occurredOn,
      description,
    };
    if (isEdit) await update.mutateAsync({ id: editing!.id, ...payload });
    else await create.mutateAsync(payload);
    onClose();
  };

  const onDelete = async () => {
    if (!editing) return;
    if (!confirm("Excluir esta transação?")) return;
    await remove.mutateAsync(editing.id);
    onClose();
  };

  return (
    <Sheet title={isEdit ? "Editar transação" : "Nova transação"} onClose={onClose}>
      <form onSubmit={submit}>
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
          <input
            type="text"
            inputMode="decimal"
            required
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            onBlur={() => {
              const n = parseBrlAmount(amount);
              if (amount !== "" && !isNaN(n)) setAmount(formatBrlInput(n));
            }}
            className={inputCls}
            placeholder="0,00"
          />
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
          <input value={description} onChange={(e) => setDescription(e.target.value)} className={inputCls} placeholder="Ex.: Salário, Mercado…" />
        </Field>
        <Field label="Data">
          <input type="date" value={occurredOn} onChange={(e) => setOccurredOn(e.target.value)} className={inputCls} />
        </Field>

        <button type="submit" disabled={busy} className="mt-2 w-full rounded-xl bg-emerald-500 py-3 font-semibold text-slate-900 disabled:opacity-60">
          {busy ? "Salvando…" : "Salvar"}
        </button>

        {isEdit && (
          <button type="button" onClick={onDelete} disabled={busy} className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl bg-slate-900 py-3 font-medium text-rose-400 disabled:opacity-60">
            <Trash2 size={18} /> Excluir
          </button>
        )}
      </form>
    </Sheet>
  );
}
