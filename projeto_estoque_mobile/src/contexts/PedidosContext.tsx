import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useContext, useEffect, useState } from 'react';
import { PEDIDOS_MOCK, type Pedido } from '@/data/pedidos';

const CHAVE = 'pedidos_v1';

type PedidosContextType = {
  pedidos: Pedido[];
  carregando: boolean;
  obterPedido: (id: string) => Pedido | undefined;
  adicionarPedido: (pedido: Pedido) => Promise<void>;
  cancelarPedido: (id: string) => Promise<void>;
  /** Próximo número sequencial para exibição (ex.: 1026). */
  proximoNumero: () => number;
};

const PedidosContext = createContext<PedidosContextType>({} as PedidosContextType);

export function PedidosProvider({ children }: { children: React.ReactNode }) {
  // Começa com o histórico fictício para a lista não piscar vazia; o
  // AsyncStorage sobrescreve assim que a leitura termina.
  const [pedidos, setPedidos] = useState<Pedido[]>(PEDIDOS_MOCK);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    let ativo = true;

    AsyncStorage.getItem(CHAVE).then(async (bruto) => {
      if (!ativo) return;

      if (bruto) {
        try {
          setPedidos(JSON.parse(bruto));
        } catch {
          // Dados corrompidos: volta para o histórico fictício.
          await AsyncStorage.setItem(CHAVE, JSON.stringify(PEDIDOS_MOCK));
        }
      } else {
        await AsyncStorage.setItem(CHAVE, JSON.stringify(PEDIDOS_MOCK));
      }

      setCarregando(false);
    });

    return () => {
      ativo = false;
    };
  }, []);

  async function persistir(lista: Pedido[]) {
    setPedidos(lista);
    await AsyncStorage.setItem(CHAVE, JSON.stringify(lista));
  }

  function obterPedido(id: string) {
    return pedidos.find((pedido) => pedido.id === id);
  }

  async function adicionarPedido(pedido: Pedido) {
    await persistir([pedido, ...pedidos]);
  }

  async function cancelarPedido(id: string) {
    await persistir(
      pedidos.map((pedido) =>
        pedido.id === id && pedido.status === 'andamento'
          ? { ...pedido, status: 'cancelado' }
          : pedido
      )
    );
  }

  function proximoNumero() {
    return pedidos.reduce((maior, pedido) => Math.max(maior, pedido.numero), 1000) + 1;
  }

  return (
    <PedidosContext.Provider
      value={{
        pedidos,
        carregando,
        obterPedido,
        adicionarPedido,
        cancelarPedido,
        proximoNumero,
      }}>
      {children}
    </PedidosContext.Provider>
  );
}

export const usePedidos = () => useContext(PedidosContext);
