import { useEffect, useState } from "react";
import {
  Baby, Book, Briefcase, Car, Coffee, Dumbbell, Gift, Heart,
  Home, Lightbulb, Music, Plane, ShoppingCart, Smartphone,
  Sparkles, Tag, TrendingDown, TrendingUp, Utensils, Zap,
  type LucideIcon,
} from "lucide-react";
import { useAnalyzeInsights, useCategoryBreakdown, useDashboard, useInsights } from "../lib/hooks";
import type { CategoryBreakdownItem } from "../lib/types";
import { brl } from "../lib/format";
import MonthSelector, { thisMonth, type RefMonth } from "./MonthSelector";

// Mapeamento de nomes Lucide → componente. Cobre os ícones mais comuns de categoria.
const LUCIDE_MAP: Record<string, LucideIcon> = {
  utensils: Utensils, car: Car, home: Home, coffee: Coffee,
  "shopping-cart": ShoppingCart, shoppingcart: ShoppingCart,
  heart: Heart, briefcase: Briefcase, plane: Plane, music: Music,
  book: Book, dumbbell: Dumbbell, zap: Zap, baby: Baby,
  gift: Gift, smartphone: Smartphone, tag: Tag,
};

function CategoryIcon({ icon, size = 16 }: { icon: string; size?: number }) {
  // Emoji: algum caractere fora do ASCII básico (codePoint > 127)
  const isEmoji = [...icon].some(c => (c.codePointAt(0) ?? 0) > 127);
  if (isEmoji) return <span aria-hidden="true" style={{ fontSize: size }}>{icon}</span>;

  const Comp = LUCIDE_MAP[icon.toLowerCase()] ?? Tag;
  return <Comp size={size} />;
}

// ─────────────────────────────────────────────
// Sub-componente: uma linha de barra horizontal
// ─────────────────────────────────────────────
function BarRow({ item }: { item: CategoryBreakdownItem }) {
  return (
    <div className="py-3">
      {/* Linha superior: ícone + nome + valor */}
      <div className="mb-1.5 flex items-center justify-between">
        <span className="flex items-center gap-2 text-sm font-medium text-white">
          <CategoryIcon icon={item.icon} />
          {item.categoryName}
        </span>
        <span className="text-sm font-semibold text-white">{brl(item.total)}</span>
      </div>

      {/* Trilho da barra */}
      <div className="flex items-center gap-2">
        <div className="relative h-2 flex-1 overflow-hidden rounded-full bg-slate-700">
          <div
            className="absolute inset-y-0 left-0 rounded-full transition-all duration-500"
            style={{
              width: `${item.percentOfExpense}%`,
              backgroundColor: item.color,
            }}
          />
        </div>
        <span className="w-10 text-right text-xs text-slate-400">
          {item.percentOfExpense.toFixed(1)}%
        </span>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Sub-componente: mini-card receitas × despesas
// ─────────────────────────────────────────────
function IncomeExpenseRow({ year, month }: { year: number; month: number }) {
  const { data, isPending, isError } = useDashboard(year, month);

  if (isPending)
    return <div className="mb-4 rounded-2xl bg-slate-800 p-4 text-sm text-slate-500">Carregando…</div>;

  if (isError || !data)
    return (
      <div className="mb-4 rounded-2xl bg-slate-800 p-4 text-sm text-rose-400">
        Erro ao carregar resumo.
      </div>
    );

  return (
    <section className="mb-4 grid grid-cols-2 gap-3">
      <div className="rounded-2xl bg-slate-800 p-4">
        <div className="mb-1 flex items-center gap-1 text-emerald-400">
          <TrendingUp size={15} />
          <span className="text-xs">Entradas</span>
        </div>
        <p className="font-semibold text-white">{brl(data.monthIncome)}</p>
      </div>
      <div className="rounded-2xl bg-slate-800 p-4">
        <div className="mb-1 flex items-center gap-1 text-rose-400">
          <TrendingDown size={15} />
          <span className="text-xs">Saídas</span>
        </div>
        <p className="font-semibold text-white">{brl(data.monthExpense)}</p>
      </div>
    </section>
  );
}

// ─────────────────────────────────────────────
// Sub-componente: lista de barras por categoria
// ─────────────────────────────────────────────
function CategoryBarChart({ year, month }: { year: number; month: number }) {
  const { data, isPending, isError } = useCategoryBreakdown(year, month);

  if (isPending)
    return <p className="text-sm text-slate-500">Carregando categorias…</p>;

  if (isError)
    return <p className="text-sm text-rose-400">Erro ao carregar breakdown. Verifique se a API está rodando.</p>;

  if (!data || data.length === 0)
    return (
      <div className="rounded-2xl bg-slate-800 p-6 text-center">
        <p className="text-sm text-slate-400">Nenhum gasto registrado neste mês.</p>
      </div>
    );

  return (
    <section className="rounded-2xl bg-slate-800 px-4">
      {data.map((item, idx) => (
        <div key={item.categoryId}>
          <BarRow item={item} />
          {idx < data.length - 1 && <hr className="border-slate-700" />}
        </div>
      ))}
    </section>
  );
}

// ─────────────────────────────────────────────
// Sub-componente: painel de insights heurísticos
// ─────────────────────────────────────────────
function InsightsPanel({ year, month }: { year: number; month: number }) {
  const { data, isPending, isError } = useInsights(year, month);
  const analyze = useAnalyzeInsights();

  // Reseta a análise de IA ao trocar de mês
  useEffect(() => { analyze.reset(); }, [year, month]); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <section className="mt-4 rounded-2xl bg-slate-800 p-4">
      <div className="mb-3 flex items-center gap-2">
        <Lightbulb size={16} className="text-yellow-400" />
        <h2 className="text-sm font-semibold text-slate-300">Insights do mês</h2>
      </div>

      {isPending && <p className="text-sm text-slate-500">Calculando…</p>}
      {isError && <p className="text-sm text-rose-400">Erro ao carregar insights.</p>}

      {data && (
        <ul className="space-y-2">
          {data.map((insight, i) => (
            <li key={i} className="flex gap-2 text-sm text-slate-300">
              <span className="mt-0.5 shrink-0 text-yellow-400">•</span>
              {insight}
            </li>
          ))}
        </ul>
      )}

      <div className="mt-4 border-t border-slate-700 pt-4">
        {analyze.isPending && (
          <p className="text-sm text-slate-400">Gerando análise com IA…</p>
        )}

        {analyze.data && (
          <p className="text-sm leading-relaxed text-slate-200">{analyze.data}</p>
        )}

        {analyze.isError && (
          <div className="space-y-3">
            <p className="text-sm text-rose-400">Não foi possível gerar a análise agora.</p>
            <button
              onClick={() => analyze.mutate({ year, month })}
              className="flex items-center gap-2 rounded-xl bg-slate-700 px-4 py-2.5 text-sm font-medium text-slate-200 transition-transform active:scale-95"
            >
              <Sparkles size={15} className="text-yellow-400" />
              Tentar novamente
            </button>
          </div>
        )}

        {!analyze.isPending && !analyze.data && !analyze.isError && (
          <button
            onClick={() => analyze.mutate({ year, month })}
            className="flex items-center gap-2 rounded-xl bg-slate-700 px-4 py-2.5 text-sm font-medium text-slate-200 transition-transform active:scale-95"
          >
            <Sparkles size={15} className="text-yellow-400" />
            Gerar análise com IA
          </button>
        )}
      </div>
    </section>
  );
}

// ─────────────────────────────────────────────
// Tela principal de Análise
// ─────────────────────────────────────────────
export default function AnalysisScreen() {
  const [refMonth, setRefMonth] = useState<RefMonth>(thisMonth());

  return (
    <>
      <h1 className="mb-4 text-xl font-bold text-white">Análise</h1>

      <MonthSelector value={refMonth} onChange={setRefMonth} />

      <IncomeExpenseRow year={refMonth.year} month={refMonth.month} />

      <h2 className="mb-3 text-sm font-semibold text-slate-300">Gastos por categoria</h2>
      <CategoryBarChart year={refMonth.year} month={refMonth.month} />

      <InsightsPanel year={refMonth.year} month={refMonth.month} />
    </>
  );
}
