export const STATUS_PEDIDO = [
  'PENDENTE',
  'PAGO',
  'PROCESSANDO',
  'PRONTO_PARA_RETIRADA',
  'RETIRADO',
  'CANCELADO',
] as const;

export type StatusPedido = (typeof STATUS_PEDIDO)[number];

export type ItemPedido = {
  produtoId: number;
  nome: string;
  preco: number;
  qtd: number;
};

export type Pedido = {
  /** Id do pedido na API; também é o número exibido na interface. */
  id: number;
  numero: number;
  /** Data de criação em ISO 8601. */
  criadoEm: string;
  total: number;
  status: StatusPedido;
  statusLabel: string;
  statusClasse: string;
  observacao: string | null;
  itens: ItemPedido[];
};

export const STATUS_LABEL: Record<StatusPedido, string> = {
  PENDENTE: 'Pendente',
  PAGO: 'Pago',
  PROCESSANDO: 'Preparando',
  PRONTO_PARA_RETIRADA: 'Pronto para retirada',
  RETIRADO: 'Retirado',
  CANCELADO: 'Cancelado',
};

/** Converte os status antigos usados antes da retirada na loja. */
const STATUS_LEGADO: Record<string, StatusPedido> = {
  ENVIADO: 'PRONTO_PARA_RETIRADA',
  ENTREGUE: 'RETIRADO',
};

export function normalizarStatus(valor: string | null | undefined): StatusPedido {
  const status = (valor ?? '').trim().toUpperCase();

  if (STATUS_LEGADO[status]) return STATUS_LEGADO[status];

  return (STATUS_PEDIDO as readonly string[]).includes(status)
    ? (status as StatusPedido)
    : 'PENDENTE';
}
