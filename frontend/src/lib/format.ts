export const brl = (value: number) =>
  value.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

export const shortDate = (iso: string) => {
  const [year, month, day] = iso.split("-");
  return `${day}/${month}/${year.slice(2)}`;
};

/** Formata um número para exibição no campo de valor (ex: 2602.49 → "2.602,49"). */
export const formatBrlInput = (value: number): string =>
  value.toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/**
 * Converte string no formato pt-BR para número JS.
 * Aceita: "2.602,49", "2602,49", "2602.49", "2602"
 */
export const parseBrlAmount = (raw: string): number => {
  const s = raw.trim();
  if (s.includes(",")) {
    // vírgula = separador decimal; pontos = separadores de milhar
    return parseFloat(s.replace(/\./g, "").replace(",", "."));
  }
  // sem vírgula: trata ponto como decimal (ex: "2602.49")
  return parseFloat(s.replace(/[^\d.]/g, ""));
};
