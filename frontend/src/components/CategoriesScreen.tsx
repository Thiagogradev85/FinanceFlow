import { useState } from "react";
import { Plus, Pencil } from "lucide-react";
import { useCategories } from "../lib/hooks";
import { CategoryKind, type CategoryDto } from "../lib/types";
import CategoryForm from "./CategoryForm";

export default function CategoriesScreen() {
  const { data, isLoading } = useCategories();
  const [form, setForm] = useState<{ editing: CategoryDto | null; kind: number } | null>(null);

  const income = data?.filter((c) => c.kind === CategoryKind.Income) ?? [];
  const expense = data?.filter((c) => c.kind === CategoryKind.Expense) ?? [];

  const Group = ({ title, items, kind }: { title: string; items: CategoryDto[]; kind: number }) => (
    <section className="mb-6">
      <div className="mb-2 flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-300">{title}</h2>
        <button onClick={() => setForm({ editing: null, kind })} className="flex items-center gap-1 text-xs text-emerald-400">
          <Plus size={14} /> Adicionar
        </button>
      </div>
      <div className="space-y-2">
        {items.map((c) => (
          <button
            key={c.id}
            onClick={() => setForm({ editing: c, kind: c.kind })}
            className="flex w-full items-center justify-between rounded-xl bg-slate-800 p-3 text-left"
          >
            <div className="flex items-center gap-2">
              <span className="h-3 w-3 rounded-full" style={{ backgroundColor: c.color }} />
              <span className="text-sm text-white">{c.name}</span>
            </div>
            <Pencil size={15} className="text-slate-500" />
          </button>
        ))}
        {items.length === 0 && <p className="text-sm text-slate-500">Nenhuma categoria.</p>}
      </div>
    </section>
  );

  return (
    <div>
      <h1 className="mb-4 text-xl font-bold text-white">Categorias</h1>
      {isLoading && <p className="text-sm text-slate-500">Carregando…</p>}
      <Group title="Despesas" items={expense} kind={CategoryKind.Expense} />
      <Group title="Receitas" items={income} kind={CategoryKind.Income} />

      {form && <CategoryForm editing={form.editing} defaultKind={form.kind} onClose={() => setForm(null)} />}
    </div>
  );
}
