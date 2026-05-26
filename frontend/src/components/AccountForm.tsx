import { useState, type FormEvent } from "react";
import { Trash2 } from "lucide-react";
import { useCreateAccount, useDeleteAccount, useUpdateAccount } from "../lib/hooks";
import type { AccountDto } from "../lib/types";
import { Field, Sheet, inputCls } from "./ui";
import { formatBrlInput, parseBrlAmount } from "../lib/format";

const ACCOUNT_TYPES = [
  { value: 1, label: "Conta corrente" },
  { value: 2, label: "Poupança" },
  { value: 3, label: "Dinheiro / Carteira" },
  { value: 4, label: "Cartão de crédito" },
  { value: 5, label: "Investimentos" },
];

export default function AccountForm({
  editing,
  onClose,
}: {
  editing: AccountDto | null;
  onClose: () => void;
}) {
  const create = useCreateAccount();
  const update = useUpdateAccount();
  const remove = useDeleteAccount();

  const isEdit = editing !== null;
  const [name, setName] = useState(editing?.name ?? "");
  const [type, setType] = useState<number>(editing?.type ?? 1);
  const [openingBalance, setOpeningBalance] = useState(editing ? formatBrlInput(editing.openingBalance) : "0,00");

  const busy = create.isPending || update.isPending || remove.isPending;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    if (isEdit) await update.mutateAsync({ id: editing!.id, name, openingBalance: parseBrlAmount(openingBalance) });
    else await create.mutateAsync({ name, type, currency: "BRL", openingBalance: parseBrlAmount(openingBalance) });
    onClose();
  };

  const onDelete = async () => {
    if (!editing) return;
    if (!confirm("Excluir esta conta? (ela será arquivada)")) return;
    await remove.mutateAsync(editing.id);
    onClose();
  };

  return (
    <Sheet title={isEdit ? "Editar conta" : "Nova conta"} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="Nome">
          <input required value={name} onChange={(e) => setName(e.target.value)} className={inputCls} placeholder="Ex.: Nubank, Carteira…" />
        </Field>

        {!isEdit && (
          <Field label="Tipo">
            <select value={type} onChange={(e) => setType(Number(e.target.value))} className={inputCls}>
              {ACCOUNT_TYPES.map((t) => (
                <option key={t.value} value={t.value}>{t.label}</option>
              ))}
            </select>
          </Field>
        )}

        {!isEdit && (
          <Field label="Saldo inicial">
            <input
              type="text"
              inputMode="decimal"
              value={openingBalance}
              onChange={(e) => setOpeningBalance(e.target.value)}
              onBlur={() => {
                const n = parseBrlAmount(openingBalance);
                if (!isNaN(n)) setOpeningBalance(formatBrlInput(n));
              }}
              className={inputCls}
              placeholder="0,00"
            />
          </Field>
        )}

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
