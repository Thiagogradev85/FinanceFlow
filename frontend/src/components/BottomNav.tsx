import { BarChart2, Home, MessageCircle, Receipt, Tags, Wallet } from "lucide-react";

export type Tab = "home" | "transactions" | "analysis" | "chat" | "categories" | "accounts";

const TABS: { id: Tab; label: string; Icon: typeof Home }[] = [
  { id: "home",         label: "Início",     Icon: Home         },
  { id: "transactions", label: "Transações", Icon: Receipt      },
  { id: "analysis",     label: "Análise",    Icon: BarChart2    },
  { id: "chat",         label: "IA",         Icon: MessageCircle},
  { id: "categories",   label: "Categorias", Icon: Tags         },
  { id: "accounts",     label: "Contas",     Icon: Wallet       },
];

export default function BottomNav({ tab, onChange }: { tab: Tab; onChange: (t: Tab) => void }) {
  return (
    <nav className="fixed bottom-0 left-1/2 z-10 w-full max-w-md -translate-x-1/2 border-t border-slate-800 bg-slate-900/95 backdrop-blur">
      <div className="grid grid-cols-6">
        {TABS.map(({ id, label, Icon }) => (
          <button
            key={id}
            onClick={() => onChange(id)}
            className={`flex flex-col items-center gap-1 py-2 text-xs ${tab === id ? "text-emerald-400" : "text-slate-500"}`}
          >
            <Icon size={20} />
            {label}
          </button>
        ))}
      </div>
    </nav>
  );
}
