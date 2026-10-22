import { produtos } from '@/data/mock';

export type StatusPedido = 'andamento' | 'entregue' | 'cancelado';

export type ItemPedido = {
  produtoId: string;
  nome: string;
  preco: number;
  qtd: number;
};

export type Pedido = {
  /** Identificador usado na rota `/pedido/[id]`. */
  id: string;
  /** Número curto exibido na interface (ex.: 1024). */
  numero: number;
  /** Data de criação em ISO 8601. */
  criadoEm: string;
  total: number;
  status: StatusPedido;
  itens: ItemPedido[];
};

export const STATUS_LABEL: Record<StatusPedido, string> = {
  andamento: 'Em andamento',
  entregue: 'Entregue',
  cancelado: 'Cancelado',
};

export const brl = (v: number) => 'R$ ' + v.toFixed(2).replace('.', ',');

/** Formata ISO em dd/mm/aaaa sem depender de Intl. */
export function formatarData(iso: string) {
  const data = new Date(iso);
  const dia = String(data.getDate()).padStart(2, '0');
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  return `${dia}/${mes}/${data.getFullYear()}`;
}

function item(produtoId: string, qtd: number): ItemPedido {
  const produto = produtos.find((p) => p.id === produtoId);
  if (!produto) throw new Error(`Produto ${produtoId} não encontrado no mock.`);
  return { produtoId, nome: produto.nome, preco: produto.preco, qtd };
}

function totalDe(itens: ItemPedido[]) {
  return itens.reduce((soma, i) => soma + i.preco * i.qtd, 0);
}

const itens1025 = [item('1', 2), item('2', 1), item('5', 2)];
const itens1024 = [item('3', 3), item('7', 4), item('8', 2)];
const itens1023 = [item('6', 2), item('9', 3)];
const itens1022 = [item('10', 1), item('4', 2)];

/**
 * Histórico fictício inicial. Usado apenas no primeiro acesso — depois os
 * pedidos passam a vir do AsyncStorage.
 */
export const PEDIDOS_MOCK: Pedido[] = [
  {
    id: '1025',
    numero: 1025,
    criadoEm: '2026-10-18T13:20:00.000Z',
    total: totalDe(itens1025),
    status: 'andamento',
    itens: itens1025,
  },
  {
    id: '1024',
    numero: 1024,
    criadoEm: '2026-10-05T16:45:00.000Z',
    total: totalDe(itens1024),
    status: 'entregue',
    itens: itens1024,
  },
  {
    id: '1023',
    numero: 1023,
    criadoEm: '2026-09-21T10:10:00.000Z',
    total: totalDe(itens1023),
    status: 'entregue',
    itens: itens1023,
  },
  {
    id: '1022',
    numero: 1022,
    criadoEm: '2026-09-02T18:05:00.000Z',
    total: totalDe(itens1022),
    status: 'cancelado',
    itens: itens1022,
  },
];
