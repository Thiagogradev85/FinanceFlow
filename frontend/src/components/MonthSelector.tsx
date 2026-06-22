import { ChevronLeft, ChevronRight } from "lucide-react";

const MONTHS = [
  "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
  "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro",
];

export interface RefMonth {
  year: number;
  month: number; // 1-12
}

export function thisMonth(): RefMonth {
  const now = new Date();
  return { year: now.getFullYear(), month: now.getMonth() + 1 };
}

/** Cabeçalho ‹ Mês Ano › — as setas avançam/voltam um mês (com virada de ano). */
export default function MonthSelector({
  value,
  onChange,
}: {
  value: RefMonth;
  onChange: (m: RefMonth) => void;
}) {
  const shift = (delta: number) => {
    const zeroBased = value.month - 1 + delta;
    const year = value.year + Math.floor(zeroBased / 12);
    const month = ((zeroBased % 12) + 12) % 12 + 1;
    onChange({ year, month });
  };

  return (
    <div className="mb-4 flex items-center justify-between">
      <button
        type="button"
        onClick={() => shift(-1)}
        aria-label="Mês anterior"
        className="rounded-xl bg-slate-800 p-2 text-slate-300 active:scale-95"
      >
        <ChevronLeft size={20} />
      </button>
      <span className="text-sm font-semibold text-white">
        {MONTHS[value.month - 1]} {value.year}
      </span>
      <button
        type="button"
        onClick={() => shift(1)}
        aria-label="Próximo mês"
        className="rounded-xl bg-slate-800 p-2 text-slate-300 active:scale-95"
      >
        <ChevronRight size={20} />
      </button>
    </div>
  );
}
