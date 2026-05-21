export const brl = (value: number) =>
  value.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

export const shortDate = (iso: string) => {
  const [year, month, day] = iso.split("-");
  return `${day}/${month}/${year.slice(2)}`;
};
