import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';
import { enviarPedido } from '@/sync/syncQueue';

const brl = (v: number) => 'R$ ' + v.toFixed(2).replace('.', ',');

export default function ConfirmarPedido() {
  const router = useRouter();
  const { itens, subtotal, frete, total, limpar } = useCart();

  async function finalizar() {
  const pedido = {
    localId: Date.now().toString(),
    itens: itens.map((i) => ({ produtoId: i.produto.id, qtd: i.qtd })),
    total,
    criadoEm: new Date().toISOString(),
  };
  await enviarPedido(pedido);
  limpar();
  router.replace(`/pedido/${pedido.localId}`);
}

  return (
    <View style={s.container}>
      <View style={s.header}>
        <TouchableOpacity onPress={() => router.back()}>
          <Ionicons name="arrow-back" size={24} color={colors.text} />
        </TouchableOpacity>
        <Text style={s.titulo}>Confirmar pedido</Text>
      </View>

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
        <View style={s.linha}><Text style={s.muted}>Produtos</Text><Text style={s.direita}>{brl(subtotal)}</Text></View>
        <View style={s.linha}><Text style={s.muted}>Frete</Text><Text style={s.direita}>{brl(frete)}</Text></View>
        <View style={s.linha}><Text style={s.total}>Total</Text><Text style={[s.total, s.direita]}>{brl(total)}</Text></View>
      </View>

      <TouchableOpacity style={s.botao} onPress={finalizar}>
        <Text style={s.botaoTxt}>Finalizar pedido</Text>
      </TouchableOpacity>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  header: { flexDirection: 'row', alignItems: 'center', gap: 12, marginBottom: 16 },
  titulo: { fontSize: 18, fontWeight: '700', color: colors.text },
  secao: { fontWeight: '700', marginTop: 12, marginBottom: 8, color: colors.text },
  card: { backgroundColor: colors.card, borderRadius: 10, padding: 12 },
  linha: { flexDirection: 'row', alignItems: 'center', marginBottom: 4 },
  texto: { color: colors.text },
  muted: { color: colors.muted, flex: 1 },
  direita: { textAlign: 'right' },
  total: { fontWeight: '700', flex: 1, fontSize: 16 },
  botao: { backgroundColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 24 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});