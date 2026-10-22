import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import {
  ActivityIndicator,
  FlatList,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { ScreenLayout } from '@/components/screen-layout';
import { StatusPedidoChip } from '@/components/status-pedido';
import { usePedidos } from '@/contexts/PedidosContext';
import { brl, formatarData, type Pedido } from '@/data/pedidos';
import { colors } from '@/theme';

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
  const { pedidos, carregando } = usePedidos();

  const ordenados = [...pedidos].sort((a, b) => (a.criadoEm < b.criadoEm ? 1 : -1));

  if (carregando) {
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
        keyExtractor={(pedido) => pedido.id}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={
          <View style={s.vazio}>
            <Ionicons name="receipt-outline" size={48} color={colors.muted} />
            <Text style={s.vazioTitulo}>Nenhuma compra ainda</Text>
            <Text style={s.vazioTexto}>Seus pedidos aparecerão aqui.</Text>
          </View>
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

  vazio: { alignItems: 'center', paddingVertical: 48, gap: 6 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text, marginTop: 8 },
  vazioTexto: { color: colors.muted },
});
