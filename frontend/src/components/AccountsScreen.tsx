import { useMemo, useState } from "react";
import { Plus, Pencil, Wallet, Star } from "lucide-react";
import { useAccountBalances, useAccounts, useSetPrimaryAccount } from "../lib/hooks";
import { brl } from "../lib/format";
import type { AccountBalanceDto, AccountDto } from "../lib/types";
import AccountForm from "./AccountForm";

export default function AccountsScreen() {
  const { data, isLoading } = useAccounts();
  const { data: balances } = useAccountBalances();
  const setPrimary = useSetPrimaryAccount();
  const [form, setForm] = useState<{ editing: AccountDto | null } | null>(null);

  // Uma única requisição traz o saldo de todas as contas; aqui só indexamos por id
  // pra cada linha achar o próprio saldo em O(1) em vez de cada uma buscar o dela.
  const balanceById = useMemo(
    () => new Map((balances ?? []).map((b) => [b.accountId, b])),
    [balances],
  );

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
          <AccountRow
            key={a.id}
            account={a}
            balance={balanceById.get(a.id)}
            onEdit={() => setForm({ editing: a })}
            onSetPrimary={() => setPrimary.mutate(a.id)}
            settingPrimary={setPrimary.isPending}
          />
        ))}
        {data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma conta. Crie a primeira.</p>}
      </div>

      {form && <AccountForm editing={form.editing} onClose={() => setForm(null)} />}
    </div>
  );
}

function AccountRow({
  account: a,
  balance,
  onEdit,
  onSetPrimary,
  settingPrimary,
}: {
  account: AccountDto;
  balance: AccountBalanceDto | undefined;
  onEdit: () => void;
  onSetPrimary: () => void;
  settingPrimary: boolean;
}) {
  return (
    <div className="flex w-full items-center justify-between rounded-xl bg-slate-800 p-3">
      <button onClick={onEdit} className="flex min-w-0 flex-1 items-center gap-2 text-left">
        <span className="rounded-lg bg-slate-700 p-2 text-slate-300"><Wallet size={16} /></span>
        <div className="min-w-0">
          <p className="flex items-center gap-1.5 text-sm text-white">
            <span className="truncate">{a.name}</span>
            {a.isPrimary && <span className="shrink-0 text-[10px] font-medium text-amber-400">Principal</span>}
          </p>
          <p className="text-xs text-slate-500">Saldo: {balance ? brl(balance.balance) : "…"}</p>
        </div>
      </button>
      <div className="flex items-center gap-1">
        <button
          onClick={() => !a.isPrimary && onSetPrimary()}
          disabled={a.isPrimary || settingPrimary}
          title={a.isPrimary ? "Conta principal" : "Tornar principal"}
          className="p-2"
        >
          <Star size={18} className={a.isPrimary ? "fill-amber-400 text-amber-400" : "text-slate-600"} />
        </button>
        <button onClick={onEdit} className="p-2" title="Editar">
          <Pencil size={15} className="text-slate-500" />
        </button>
      </div>
    </div>
  );
}
