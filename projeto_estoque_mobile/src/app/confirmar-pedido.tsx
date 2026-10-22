import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useCart } from '@/contexts/CartContext';
import { usePedidos } from '@/contexts/PedidosContext';
import { brl, type Pedido } from '@/data/pedidos';
import { colors } from '@/theme';
import { ScreenLayout } from '@/components/screen-layout';
import { enviarPedido } from '@/sync/syncQueue';

export default function ConfirmarPedido() {
  const router = useRouter();
  const { itens, subtotal, frete, total, limpar } = useCart();
  const { adicionarPedido, proximoNumero } = usePedidos();

  async function finalizar() {
    const criadoEm = new Date().toISOString();

    const pedido: Pedido = {
      id: Date.now().toString(),
      numero: proximoNumero(),
      criadoEm,
      total,
      status: 'andamento',
      itens: itens.map((i) => ({
        produtoId: i.produto.id,
        nome: i.produto.nome,
        preco: i.produto.preco,
        qtd: i.qtd,
      })),
    };

    await enviarPedido({
      localId: pedido.id,
      itens: pedido.itens.map((i) => ({ produtoId: i.produtoId, qtd: i.qtd })),
      total: pedido.total,
      criadoEm,
    });

    await adicionarPedido(pedido);
    limpar();
    router.replace(`/pedido/${pedido.id}`);
  }

  return (
    <ScreenLayout title="Confirmar pedido" subtitle="Revise antes de finalizar" mostrarVoltar>
      <ScrollView
        style={s.container}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}>
        <Text style={s.secao}>Endereço de entrega</Text>
        <View style={s.card}>
          <Text style={s.texto}>Rua das Flores, 123</Text>
          <Text style={{ color: colors.muted }}>Centro - Sua Cidade/UF</Text>
        </View>

        <Text style={s.secao}>Forma de pagamento</Text>
        <View style={[s.card, s.linha]}>
          <Ionicons name="qr-code-outline" size={20} color={colors.primary} />
          <Text style={[s.texto, { flex: 1, marginLeft: 8 }]}>Pix</Text>
          <Ionicons name="chevron-forward" size={18} color={colors.muted} />
        </View>

        <Text style={s.secao}>Resumo do pedido</Text>
        <View style={s.card}>
          <View style={s.linha}>
            <Text style={s.muted}>Produtos</Text>
            <Text style={s.direita}>{brl(subtotal)}</Text>
          </View>
          <View style={s.linha}>
            <Text style={s.muted}>Frete</Text>
            <Text style={s.direita}>{brl(frete)}</Text>
          </View>
          <View style={s.linha}>
            <Text style={s.total}>Total</Text>
            <Text style={[s.total, s.direita]}>{brl(total)}</Text>
          </View>
        </View>

        <TouchableOpacity style={s.botao} activeOpacity={0.85} onPress={finalizar}>
          <Text style={s.botaoTxt}>Finalizar pedido</Text>
        </TouchableOpacity>
      </ScrollView>
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  secao: { fontWeight: '700', marginTop: 12, marginBottom: 8, color: colors.text },
  card: { backgroundColor: colors.card, borderRadius: 10, padding: 12 },
  linha: { flexDirection: 'row', alignItems: 'center', marginBottom: 4 },
  texto: { color: colors.text },
  muted: { color: colors.muted, flex: 1 },
  direita: { textAlign: 'right' },
  total: { fontWeight: '700', flex: 1, fontSize: 16, color: colors.text },
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 24,
  },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});
