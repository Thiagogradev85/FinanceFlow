import { useState } from "react";
import { Plus } from "lucide-react";
import { useDashboard } from "./lib/hooks";
import BalanceCard from "./components/BalanceCard";
import AccountsList from "./components/AccountsList";
import TransactionList from "./components/TransactionList";
import AddTransactionForm from "./components/AddTransactionForm";

export default function App() {
  const [showForm, setShowForm] = useState(false);
  const dashboard = useDashboard();

  return (
    <div className="mx-auto min-h-screen max-w-md px-4 pb-28 pt-6">
      <header className="mb-6">
        <h1 className="text-xl font-bold text-white">FinanceFlow</h1>
        <p className="text-sm text-slate-400">Seu controle de gastos</p>
      </header>

      <BalanceCard query={dashboard} />
      <AccountsList />
      <TransactionList />

      <button
        onClick={() => setShowForm(true)}
        className="fixed bottom-6 left-1/2 flex -translate-x-1/2 items-center gap-2 rounded-full bg-emerald-500 px-6 py-3 font-semibold text-slate-900 shadow-lg shadow-emerald-500/30 active:scale-95"
      >
        <Plus size={20} /> Nova transação
      </button>

      {showForm && <AddTransactionForm onClose={() => setShowForm(false)} />}
    </div>
  );
}
