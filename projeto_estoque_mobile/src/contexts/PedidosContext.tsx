import AsyncStorage from '@react-native-async-storage/async-storage';
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { chavePedidosCache } from '@/services/armazenamento';
import { ApiError, mensagemDeErro } from '@/services/api';
import {
  buscarPedido,
  cancelarPedido as cancelarPedidoNaApi,
  criarPedido as criarPedidoNaApi,
  listarMeusPedidos,
} from '@/services/pedidos';
import { normalizarStatus, STATUS_LABEL, type Pedido } from '@/data/pedidos';

type PedidosContextType = {
  pedidos: Pedido[];
  carregando: boolean;
  erro: string | null;
  recarregar: () => Promise<void>;
  obterPedido: (id: number) => Pedido | undefined;
  carregarPedido: (id: number) => Promise<Pedido>;
  criarPedido: (
    itens: { produtoId: number; quantidade: number }[],
    observacao?: string
  ) => Promise<Pedido>;
  cancelarPedido: (id: number) => Promise<Pedido>;
};

const PedidosContext = createContext<PedidosContextType>({} as PedidosContextType);

function lerCache(bruto: string | null): Pedido[] {
  if (!bruto) return [];

  try {
    const valor = JSON.parse(bruto);

    if (!Array.isArray(valor)) return [];

    return valor.map((pedido) => {
      const status = normalizarStatus((pedido as Pedido).status);

      return { ...(pedido as Pedido), status, statusLabel: STATUS_LABEL[status] };
    });
  } catch {
    return [];
  }
}

export function PedidosProvider({ children }: { children: React.ReactNode }) {
  const { usuario } = useAuth();
  const usuarioId = usuario?.id ?? null;

  const [pedidos, setPedidos] = useState<Pedido[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const pedidosRef = useRef<Pedido[]>([]);

  useEffect(() => {
    pedidosRef.current = pedidos;
  }, [pedidos]);

  useEffect(() => {
    let ativo = true;

    if (!usuarioId) {
      pedidosRef.current = [];

      void Promise.resolve().then(() => {
        if (!ativo) return;
        setPedidos([]);
        setErro(null);
        setCarregando(false);
      });

      return () => {
        ativo = false;
      };
    }

    const chave = chavePedidosCache(usuarioId);

    AsyncStorage.getItem(chave)
      .then((bruto) => {
        if (!ativo) return;

        setCarregando(true);
        setErro(null);

        const cache = lerCache(bruto);
        if (cache.length > 0) {
          pedidosRef.current = cache;
          setPedidos(cache);
        }

        return listarMeusPedidos();
      })
      .then((lista) => {
        if (!ativo || !lista) return;

        pedidosRef.current = lista;
        setPedidos(lista);
        setErro(null);
        return AsyncStorage.setItem(chave, JSON.stringify(lista));
      })
      .catch((falha) => {
        if (!ativo) return;
        setErro(mensagemDeErro(falha));
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [usuarioId]);

  const recarregar = useCallback(async () => {
    if (!usuarioId) {
      pedidosRef.current = [];
      setPedidos([]);
      setErro(null);
      setCarregando(false);
      return;
    }

    setCarregando(true);
    setErro(null);

    try {
      const lista = await listarMeusPedidos();
      pedidosRef.current = lista;
      setPedidos(lista);
      await AsyncStorage.setItem(chavePedidosCache(usuarioId), JSON.stringify(lista));
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setCarregando(false);
    }
  }, [usuarioId]);

  function obterPedido(id: number) {
    return pedidos.find((pedido) => pedido.id === id);
  }

  const guardarPedido = useCallback(
    async (pedido: Pedido) => {
      const atuais = pedidosRef.current;
      const lista = atuais.some((item) => item.id === pedido.id)
        ? atuais.map((item) => (item.id === pedido.id ? pedido : item))
        : [pedido, ...atuais];

      pedidosRef.current = lista;
      setPedidos(lista);

      if (usuarioId) {
        await AsyncStorage.setItem(chavePedidosCache(usuarioId), JSON.stringify(lista));
      }
    },
    [usuarioId]
  );

  const carregarPedido = useCallback(
    async (id: number) => {
      try {
        const pedido = await buscarPedido(id);
        await guardarPedido(pedido);
        return pedido;
      } catch (falha) {
        const local = pedidosRef.current.find((pedido) => pedido.id === id);

        if (!local) throw falha;

        const podeUsarCache =
          !(falha instanceof ApiError) || falha.status === undefined || falha.status >= 500;

        if (podeUsarCache) return local;
        throw falha;
      }
    },
    [guardarPedido]
  );

  const criarPedido = useCallback(
    async (itens: { produtoId: number; quantidade: number }[], observacao?: string) => {
      if (!usuarioId) throw new Error('Entre na sua conta para finalizar o pedido.');

      const pedido = await criarPedidoNaApi(itens, observacao);
      await guardarPedido(pedido);

      return pedido;
    },
    [guardarPedido, usuarioId]
  );

  const cancelarPedido = useCallback(
    async (id: number) => {
      if (!usuarioId) throw new Error('Entre na sua conta para cancelar o pedido.');

      const pedido = await cancelarPedidoNaApi(id);
      await guardarPedido(pedido);

      return pedido;
    },
    [guardarPedido, usuarioId]
  );

  return (
    <PedidosContext.Provider
      value={{
        pedidos,
        carregando,
        erro,
        recarregar,
        obterPedido,
        carregarPedido,
        criarPedido,
        cancelarPedido,
      }}>
      {children}
    </PedidosContext.Provider>
  );
}

export const usePedidos = () => useContext(PedidosContext);
