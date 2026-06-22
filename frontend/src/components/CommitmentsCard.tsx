import { CalendarClock } from "lucide-react";
import { useCommitments } from "../lib/hooks";
import { brl } from "../lib/format";

const MONTHS = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

/** Mostra o total de parcelas já agendadas para os próximos meses. Some quando não há nada parcelado. */
export default function CommitmentsCard() {
  const { data } = useCommitments(6);
  if (!data || data.total <= 0) return null;

  return (
    <section className="mb-6 rounded-2xl bg-slate-800 p-5">
      <div className="flex items-center gap-1 text-indigo-300">
        <CalendarClock size={16} />
        <span className="text-xs">Comprometido (próximos meses)</span>
      </div>
      <p className="mb-3 mt-1 text-2xl font-bold text-white">{brl(data.total)}</p>

      <div className="space-y-1">
        {data.months.map((m) => (
          <div key={`${m.year}-${m.month}`} className="flex justify-between text-sm">
            <span className="text-slate-400">{MONTHS[m.month - 1]}/{String(m.year).slice(2)}</span>
            <span className="text-slate-200">{brl(m.amount)}</span>
          </div>
        ))}
      </div>
    </section>
  );
}
