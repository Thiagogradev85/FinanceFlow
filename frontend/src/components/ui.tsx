import type { ReactNode } from "react";
import { X } from "lucide-react";

export const inputCls =
  "w-full rounded-xl bg-slate-900 px-3 py-2 text-white outline-none focus:ring-2 focus:ring-emerald-500";

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="mb-3 block">
      <span className="mb-1 block text-xs text-slate-400">{label}</span>
      {children}
    </label>
  );
}

/** Bottom sheet (modal que sobe de baixo) — padrão de apps mobile. */
export function Sheet({ title, onClose, children }: { title: string; onClose: () => void; children: ReactNode }) {
  return (
    <div className="fixed inset-0 z-20 flex items-end bg-black/50" onClick={onClose}>
      <div
        onClick={(e) => e.stopPropagation()}
        className="max-h-[90vh] w-full overflow-y-auto rounded-t-3xl bg-slate-800 p-5"
      >
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-semibold text-white">{title}</h3>
          <button type="button" onClick={onClose} className="text-slate-400" aria-label="Fechar">
            <X />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}
