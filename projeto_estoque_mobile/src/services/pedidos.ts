import { api } from '@/services/api';
import {
  normalizarStatus,
  STATUS_LABEL,
  type ItemPedido,
  type Pedido,
} from '@/data/pedidos';

type ItemPedidoApi = {
  produtoId: number;
  nomeProduto: string;
  quantidade: number;
  precoUnitario: number;
  subtotal: number;
};

type PedidoApi = {
  id: number;
  usuarioId: number;
  cliente: string;
  data: string;
  status: string;
  statusLabel: string;
  statusClasse: string;
  totalItens: number;
  valorTotal: number;
  observacao: string | null;
  itens: ItemPedidoApi[];
};

type PaginaPedidosApi = {
  itens: PedidoApi[];
  total: number;
  pagina: number;
  totalPaginas: number;
  itensPorPagina: number;
};

function mapearItem(item: ItemPedidoApi): ItemPedido {
  return {
    produtoId: item.produtoId,
    nome: item.nomeProduto,
    preco: item.precoUnitario,
    qtd: item.quantidade,
  };
}

function mapearPedido(pedido: PedidoApi): Pedido {
  const status = normalizarStatus(pedido.status);

  return {
    id: pedido.id,
    numero: pedido.id,
    criadoEm: pedido.data,
    total: pedido.valorTotal,
    status,
    statusLabel: pedido.statusLabel || STATUS_LABEL[status],
    statusClasse: pedido.statusClasse,
    observacao: pedido.observacao,
    itens: pedido.itens.map(mapearItem),
  };
}

export async function listarMeusPedidos(): Promise<Pedido[]> {
  const pedidos: Pedido[] = [];
  let pagina = 1;
  let totalPaginas = 1;

  do {
    const { data } = await api.get<PaginaPedidosApi>('/pedidos/meus', {
      params: { pagina, itensPorPagina: 100 },
    });

    pedidos.push(...data.itens.map(mapearPedido));
    totalPaginas = data.totalPaginas;
    pagina += 1;
  } while (pagina <= totalPaginas && pagina <= 20);

  return pedidos;
}

export async function buscarPedido(id: number): Promise<Pedido> {
  const { data } = await api.get<PedidoApi>(`/pedidos/${id}`);
  return mapearPedido(data);
}

export async function criarPedido(
  itens: { produtoId: number; quantidade: number }[],
  observacao?: string
): Promise<Pedido> {
  const { data } = await api.post<PedidoApi>('/pedidos', {
    itens,
    observacao: observacao || undefined,
  });

  return mapearPedido(data);
}

export async function cancelarPedido(id: number): Promise<Pedido> {
  const { data } = await api.patch<PedidoApi>(`/pedidos/${id}/status`, {
    status: 'CANCELADO',
  });

  return mapearPedido(data);
}
