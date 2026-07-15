import { useState, useEffect, type FormEvent } from "react";
import { useAccounts, useCreateTransfer } from "../lib/hooks";
import { Field, Sheet, inputCls } from "./ui";
import { formatBrlInput, parseBrlAmount } from "../lib/format";

export default function TransferForm({ onClose }: { onClose: () => void }) {
  const { data: accounts } = useAccounts();
  const create = useCreateTransfer();

  const [fromAccountId, setFrom] = useState("");
  const [toAccountId, setTo] = useState("");
  const [amount, setAmount] = useState("");
  const [occurredOn, setOccurredOn] = useState(new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState("");

  useEffect(() => {
    if (!accounts || accounts.length < 2) return;
    const primary = accounts.find((a) => a.isPrimary)?.id ?? accounts[0].id;
    const second = accounts.find((a) => a.id !== primary)?.id ?? accounts[1].id;
    setFrom(primary);
    setTo(second);
  }, [accounts]);

  const busy = create.isPending;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    await create.mutateAsync({
      fromAccountId,
      toAccountId,
      amount: parseBrlAmount(amount),
      occurredOn,
      description,
    });
    onClose();
  };

  if (!accounts || accounts.length < 2) {
    return (
      <Sheet title="Transferência" onClose={onClose}>
        <p className="text-sm text-slate-400">Você precisa de pelo menos 2 contas para fazer uma transferência.</p>
      </Sheet>
    );
  }

  return (
    <Sheet title="Transferência entre contas" onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="De (conta origem)">
          <select value={fromAccountId} onChange={(e) => setFrom(e.target.value)} className={inputCls}>
            {accounts.map((a) => (
              <option key={a.id} value={a.id}>{a.name}</option>
            ))}
          </select>
        </Field>

        <Field label="Para (conta destino)">
          <select value={toAccountId} onChange={(e) => setTo(e.target.value)} className={inputCls}>
            {accounts.filter((a) => a.id !== fromAccountId).map((a) => (
              <option key={a.id} value={a.id}>{a.name}</option>
            ))}
          </select>
        </Field>

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

        <Field label="Data">
          <input type="date" value={occurredOn} onChange={(e) => setOccurredOn(e.target.value)} className={inputCls} />
        </Field>

        <Field label="Descrição">
          <input value={description} onChange={(e) => setDescription(e.target.value)} className={inputCls} placeholder="Ex.: Reserva de emergência" />
        </Field>

        <button
          type="submit"
          disabled={busy || fromAccountId === toAccountId}
          className="mt-2 w-full rounded-xl bg-sky-500 py-3 font-semibold text-slate-900 disabled:opacity-60"
        >
          {busy ? "Transferindo…" : "Transferir"}
        </button>
      </form>
    </Sheet>
  );
}
