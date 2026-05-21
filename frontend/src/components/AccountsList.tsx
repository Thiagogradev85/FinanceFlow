import { Wallet } from "lucide-react";
import { useAccounts } from "../lib/hooks";
import { brl } from "../lib/format";

export default function AccountsList() {
  const { data, isLoading } = useAccounts();

  return (
    <section className="mb-6">
      <h2 className="mb-2 text-sm font-semibold text-slate-300">Contas</h2>
      {isLoading && <p className="text-sm text-slate-500">Carregando…</p>}
      <div className="space-y-2">
        {data?.map((account) => (
          <div key={account.id} className="flex items-center justify-between rounded-xl bg-slate-800 p-3">
            <div className="flex items-center gap-2">
              <span className="rounded-lg bg-slate-700 p-2 text-slate-300">
                <Wallet size={16} />
              </span>
              <span className="text-sm text-white">{account.name}</span>
            </div>
            <span className="text-sm text-slate-300">{brl(account.openingBalance)}</span>
          </div>
        ))}
      </div>
    </section>
  );
}
