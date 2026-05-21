import { useState } from "react";
import { Plus } from "lucide-react";
import { useDashboard } from "./lib/hooks";
import type { TransactionDto } from "./lib/types";
import BalanceCard from "./components/BalanceCard";
import TransactionList from "./components/TransactionList";
import TransactionForm from "./components/TransactionForm";
import CategoriesScreen from "./components/CategoriesScreen";
import AccountsScreen from "./components/AccountsScreen";
import BottomNav, { type Tab } from "./components/BottomNav";

export default function App() {
  const [tab, setTab] = useState<Tab>("home");
  const [form, setForm] = useState<{ editing: TransactionDto | null } | null>(null);
  const dashboard = useDashboard();

  const openAdd = () => setForm({ editing: null });
  const openEdit = (t: TransactionDto) => setForm({ editing: t });

  const showFab = tab === "home" || tab === "transactions";

  return (
    <div className="mx-auto min-h-screen max-w-md px-4 pb-24 pt-6">
      {tab === "home" && (
        <>
          <header className="mb-6">
            <h1 className="text-xl font-bold text-white">FinanceFlow</h1>
            <p className="text-sm text-slate-400">Seu controle de gastos</p>
          </header>
          <BalanceCard query={dashboard} />
          <h2 className="mb-2 text-sm font-semibold text-slate-300">Últimas transações</h2>
          <TransactionList onSelect={openEdit} limit={5} />
        </>
      )}

      {tab === "transactions" && (
        <>
          <h1 className="mb-4 text-xl font-bold text-white">Transações</h1>
          <TransactionList onSelect={openEdit} />
        </>
      )}

      {tab === "categories" && <CategoriesScreen />}
      {tab === "accounts" && <AccountsScreen />}

      {showFab && (
        <button
          onClick={openAdd}
          className="fixed bottom-20 left-1/2 z-10 flex -translate-x-1/2 items-center gap-2 rounded-full bg-emerald-500 px-6 py-3 font-semibold text-slate-900 shadow-lg shadow-emerald-500/30 active:scale-95"
        >
          <Plus size={20} /> Nova transação
        </button>
      )}

      {form && <TransactionForm editing={form.editing} onClose={() => setForm(null)} />}

      <BottomNav tab={tab} onChange={setTab} />
    </div>
  );
}
