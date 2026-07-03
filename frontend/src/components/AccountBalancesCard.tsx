import { Wallet } from "lucide-react";
import { useAccountBalances, useAccounts } from "../lib/hooks";
import { brl } from "../lib/format";

/** Saldo de cada conta. Some com 1 conta só (o total do BalanceCard já basta nesse caso). */
export default function AccountBalancesCard() {
  const { data: accounts } = useAccounts();
  const { data: balances } = useAccountBalances();

  if (!accounts || accounts.length < 2 || !balances) return null;

  const balanceByAccountId = new Map(balances.map((b) => [b.accountId, b.balance]));

  return (
    <section className="mb-6 rounded-2xl bg-slate-800 p-5">
      <div className="flex items-center gap-1 text-slate-400">
        <Wallet size={16} />
        <span className="text-xs">Saldo por conta</span>
      </div>
      <div className="mt-3 space-y-2">
        {accounts.map((a) => (
          <div key={a.id} className="flex items-center justify-between text-sm">
            <span className="truncate text-slate-300">{a.name}</span>
            <span className="font-semibold text-white">{brl(balanceByAccountId.get(a.id) ?? 0)}</span>
          </div>
        ))}
      </div>
    </section>
  );
}
