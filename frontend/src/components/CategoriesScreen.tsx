import { useState } from "react";
import { Plus, Pencil } from "lucide-react";
import { useCategories } from "../lib/hooks";
import { CategoryKind, type CategoryDto } from "../lib/types";
import CategoryForm from "./CategoryForm";

function CategoryBadge({ item, onEdit }: { item: CategoryDto; onEdit: (c: CategoryDto) => void }) {
  return (
    <button
      onClick={() => onEdit(item)}
      className="flex items-center gap-1.5 rounded-full px-3 py-1.5 text-sm font-medium text-white active:scale-95 transition-transform"
      style={{ backgroundColor: item.color + "33", border: `1px solid ${item.color}66` }}
    >
      <span className="h-2 w-2 flex-shrink-0 rounded-full" style={{ backgroundColor: item.color }} />
      {item.name}
      <Pencil size={11} className="text-white/50 ml-0.5" />
    </button>
  );
}

export default function CategoriesScreen() {
  const { data, isLoading } = useCategories();
  const [form, setForm] = useState<{ editing: CategoryDto | null; kind: number } | null>(null);

  const income = data?.filter((c) => c.kind === CategoryKind.Income) ?? [];
  const expense = data?.filter((c) => c.kind === CategoryKind.Expense) ?? [];

  const Group = ({ title, items, kind }: { title: string; items: CategoryDto[]; kind: number }) => (
    <section className="mb-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-300">{title}</h2>
        <button onClick={() => setForm({ editing: null, kind })} className="flex items-center gap-1 text-xs text-emerald-400">
          <Plus size={14} /> Adicionar
        </button>
      </div>
      <div className="flex flex-wrap gap-2">
        {items.map((c) => (
          <CategoryBadge key={c.id} item={c} onEdit={(cat) => setForm({ editing: cat, kind: cat.kind })} />
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
