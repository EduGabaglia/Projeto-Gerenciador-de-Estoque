import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { api } from '@/services/api';

const CHAVE = 'fila_pedidos';

export type PedidoPendente = {
  localId: string;
  itens: { produtoId: string; qtd: number }[];
  total: number;
  criadoEm: string;
};

async function lerFila(): Promise<PedidoPendente[]> {
  const bruto = await AsyncStorage.getItem(CHAVE);
  return bruto ? JSON.parse(bruto) : [];
}

async function salvarFila(fila: PedidoPendente[]) {
  await AsyncStorage.setItem(CHAVE, JSON.stringify(fila));
}

// Tenta enviar; se estiver sem internet ou der erro, guarda na fila
export async function enviarPedido(pedido: PedidoPendente): Promise<'enviado' | 'pendente'> {
  const rede = await NetInfo.fetch();
  if (rede.isConnected) {
    try {
      await api.post('/pedidos', pedido);
      return 'enviado';
    } catch {
      // cai na fila abaixo
    }
  }
  const fila = await lerFila();
  await salvarFila([...fila, pedido]);
  return 'pendente';
}

// Envia tudo o que ficou pendente
export async function sincronizar() {
  const fila = await lerFila();
  const restantes: PedidoPendente[] = [];
  for (const pedido of fila) {
    try {
      await api.post('/pedidos', pedido);
    } catch {
      restantes.push(pedido);
    }
  }
  await salvarFila(restantes);
}

// Chama sincronizar() automaticamente quando a internet volta
export function iniciarSincronizacaoAutomatica() {
  return NetInfo.addEventListener((estado) => {
    if (estado.isConnected) sincronizar();
  });
}