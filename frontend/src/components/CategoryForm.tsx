import { useState, type FormEvent } from "react";
import { Trash2 } from "lucide-react";
import { useCreateCategory, useDeleteCategory, useUpdateCategory } from "../lib/hooks";
import { CategoryKind, type CategoryDto } from "../lib/types";
import { Field, Sheet, inputCls } from "./ui";

const PRESET_COLORS = ["#ef4444", "#f59e0b", "#22c55e", "#3b82f6", "#a855f7", "#ec4899", "#14b8a6", "#888888"];

export default function CategoryForm({
  editing,
  defaultKind,
  onClose,
}: {
  editing: CategoryDto | null;
  defaultKind: number;
  onClose: () => void;
}) {
  const create = useCreateCategory();
  const update = useUpdateCategory();
  const remove = useDeleteCategory();

  const isEdit = editing !== null;
  const [name, setName] = useState(editing?.name ?? "");
  const [kind, setKind] = useState<number>(editing?.kind ?? defaultKind);
  const [color, setColor] = useState(editing?.color ?? PRESET_COLORS[0]);
  const [icon] = useState(editing?.icon ?? "tag");

  const busy = create.isPending || update.isPending || remove.isPending;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    if (isEdit) await update.mutateAsync({ id: editing!.id, name, color, icon });
    else await create.mutateAsync({ name, kind, color, icon });
    onClose();
  };

  const onDelete = async () => {
    if (!editing) return;
    if (!confirm("Excluir esta categoria? (ela será arquivada)")) return;
    await remove.mutateAsync(editing.id);
    onClose();
  };

  return (
    <Sheet title={isEdit ? "Editar categoria" : "Nova categoria"} onClose={onClose}>
      <form onSubmit={submit}>
        {!isEdit && (
          <div className="mb-3 grid grid-cols-2 gap-2">
            <button
              type="button"
              onClick={() => setKind(CategoryKind.Expense)}
              className={`rounded-xl py-2 text-sm font-medium ${kind === CategoryKind.Expense ? "bg-rose-500 text-white" : "bg-slate-900 text-slate-400"}`}
            >
              Despesa
            </button>
            <button
              type="button"
              onClick={() => setKind(CategoryKind.Income)}
              className={`rounded-xl py-2 text-sm font-medium ${kind === CategoryKind.Income ? "bg-emerald-500 text-slate-900" : "bg-slate-900 text-slate-400"}`}
            >
              Receita
            </button>
          </div>
        )}

        <Field label="Nome">
          <input required value={name} onChange={(e) => setName(e.target.value)} className={inputCls} placeholder="Ex.: Mercado, Aluguel, Salário…" />
        </Field>

        <Field label="Cor">
          <div className="flex flex-wrap gap-2">
            {PRESET_COLORS.map((c) => (
              <button
                key={c}
                type="button"
                onClick={() => setColor(c)}
                className={`h-8 w-8 rounded-full border-2 ${color === c ? "border-white" : "border-transparent"}`}
                style={{ backgroundColor: c }}
                aria-label={c}
              />
            ))}
          </div>
        </Field>

        <button type="submit" disabled={busy} className="mt-2 w-full rounded-xl bg-emerald-500 py-3 font-semibold text-slate-900 disabled:opacity-60">
          {busy ? "Salvando…" : "Salvar"}
        </button>

        {isEdit && (
          <button type="button" onClick={onDelete} disabled={busy} className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl bg-slate-900 py-3 font-medium text-rose-400 disabled:opacity-60">
            <Trash2 size={18} /> Excluir
          </button>
        )}
      </form>
    </Sheet>
  );
}
