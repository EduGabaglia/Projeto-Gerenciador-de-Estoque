export const brl = (valor: number) => 'R$ ' + valor.toFixed(2).replace('.', ',');

/** Formata uma data ISO em dd/mm/aaaa sem depender de Intl. */
export function formatarData(iso: string) {
  const data = new Date(iso);

  if (Number.isNaN(data.getTime())) return '—';

  const dia = String(data.getDate()).padStart(2, '0');
  const mes = String(data.getMonth() + 1).padStart(2, '0');

  return `${dia}/${mes}/${data.getFullYear()}`;
}
