import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useState } from 'react';
import { ScreenLayout } from '@/components/screen-layout';
import { StatusPedidoChip } from '@/components/status-pedido';
import { usePedidos } from '@/contexts/PedidosContext';
import type { Pedido } from '@/data/pedidos';
import { colors } from '@/theme';
import { brl, formatarData } from '@/utils/formato';

function CartaoPedido({ pedido, onPress }: { pedido: Pedido; onPress: () => void }) {
  const unidades = pedido.itens.reduce((soma, item) => soma + item.qtd, 0);

  return (
    <TouchableOpacity style={s.card} activeOpacity={0.8} onPress={onPress}>
      <View style={s.topo}>
        <Text style={s.numero}>Pedido #{pedido.numero}</Text>
        <StatusPedidoChip status={pedido.status} />
      </View>

      <Text style={s.detalhe}>
        {formatarData(pedido.criadoEm)} • {unidades} {unidades === 1 ? 'item' : 'itens'}
      </Text>

      <View style={s.rodape}>
        <Text style={s.total}>{brl(pedido.total)}</Text>
        <Ionicons name="chevron-forward" size={18} color={colors.muted} />
      </View>
    </TouchableOpacity>
  );
}

export default function MinhasCompras() {
  const router = useRouter();
  const { pedidos, carregando, erro, recarregar } = usePedidos();
  const [atualizando, setAtualizando] = useState(false);

  const ordenados = [...pedidos].sort((a, b) => b.criadoEm.localeCompare(a.criadoEm));

  async function atualizar() {
    setAtualizando(true);
    await recarregar();
    setAtualizando(false);
  }

  if (carregando && pedidos.length === 0) {
    return (
      <ScreenLayout title="Minhas compras" mostrarVoltar voltarPara="/(tabs)/perfil">
        <View style={s.vazio}>
          <ActivityIndicator color={colors.primary} size="large" />
        </View>
      </ScreenLayout>
    );
  }

  return (
    <ScreenLayout
      title="Minhas compras"
      subtitle={`${pedidos.length} ${pedidos.length === 1 ? 'pedido' : 'pedidos'}`}
      mostrarVoltar
      voltarPara="/(tabs)/perfil">
      <FlatList
        style={s.container}
        data={ordenados}
        keyExtractor={(pedido) => String(pedido.id)}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={atualizando} onRefresh={atualizar} tintColor={colors.primary} />
        }
        ListHeaderComponent={
          erro ? (
            <View style={s.erroBox}>
              <Ionicons name="cloud-offline-outline" size={20} color={colors.danger} />
              <View style={s.erroTextoBox}>
                <Text style={s.erroTitulo}>Não foi possível atualizar os pedidos</Text>
                <Text style={s.erroTexto}>{erro}</Text>
              </View>
              <TouchableOpacity onPress={() => void recarregar()}>
                <Text style={s.erroAcao}>Tentar</Text>
              </TouchableOpacity>
            </View>
          ) : null
        }
        ListEmptyComponent={
          erro ? null : (
            <View style={s.vazio}>
              <Ionicons name="receipt-outline" size={48} color={colors.muted} />
              <Text style={s.vazioTitulo}>Nenhuma compra ainda</Text>
              <Text style={s.vazioTexto}>Seus pedidos aparecerão aqui.</Text>
            </View>
          )
        }
        renderItem={({ item }) => (
          <CartaoPedido pedido={item} onPress={() => router.push(`/pedido/${item.id}`)} />
        )}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },

  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 14,
    marginBottom: 10,
  },
  topo: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 10,
  },
  numero: { fontSize: 15, fontWeight: '700', color: colors.text },
  detalhe: { fontSize: 13, color: colors.muted, marginTop: 6 },
  rodape: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 10,
  },
  total: { fontSize: 15, fontWeight: '700', color: colors.text },

  erroBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    backgroundColor: colors.dangerBg,
    borderRadius: 10,
    padding: 12,
    marginBottom: 12,
  },
  erroTextoBox: { flex: 1 },
  erroTitulo: { color: colors.danger, fontWeight: '700', fontSize: 13 },
  erroTexto: { color: colors.danger, fontSize: 12, marginTop: 2 },
  erroAcao: { color: colors.danger, fontWeight: '700', fontSize: 12 },

  vazio: { alignItems: 'center', paddingVertical: 48, gap: 6 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text, marginTop: 8 },
  vazioTexto: { color: colors.muted },
});
