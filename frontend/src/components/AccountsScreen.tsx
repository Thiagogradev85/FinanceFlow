import { useState } from "react";
import { Plus, Pencil, Wallet } from "lucide-react";
import { useAccounts } from "../lib/hooks";
import { brl } from "../lib/format";
import type { AccountDto } from "../lib/types";
import AccountForm from "./AccountForm";

export default function AccountsScreen() {
  const { data, isLoading } = useAccounts();
  const [form, setForm] = useState<{ editing: AccountDto | null } | null>(null);

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-white">Contas</h1>
        <button onClick={() => setForm({ editing: null })} className="flex items-center gap-1 text-sm text-emerald-400">
          <Plus size={16} /> Nova
        </button>
      </div>

      {isLoading && <p className="text-sm text-slate-500">Carregando…</p>}
      <div className="space-y-2">
        {data?.map((a) => (
          <button
            key={a.id}
            onClick={() => setForm({ editing: a })}
            className="flex w-full items-center justify-between rounded-xl bg-slate-800 p-3 text-left"
          >
            <div className="flex items-center gap-2">
              <span className="rounded-lg bg-slate-700 p-2 text-slate-300"><Wallet size={16} /></span>
              <div>
                <p className="text-sm text-white">{a.name}</p>
                <p className="text-xs text-slate-500">Saldo inicial: {brl(a.openingBalance)}</p>
              </div>
            </div>
            <Pencil size={15} className="text-slate-500" />
          </button>
        ))}
        {data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma conta. Crie a primeira.</p>}
      </div>

      {form && <AccountForm editing={form.editing} onClose={() => setForm(null)} />}
    </div>
  );
}
