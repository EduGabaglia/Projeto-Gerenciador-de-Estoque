import { View, Text, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';

const brl = (v: number) => 'R$ ' + v.toFixed(2).replace('.', ',');

export default function Carrinho() {
  const router = useRouter();
  const { itens, alterarQtd, subtotal, frete, total } = useCart();

  if (itens.length === 0) {
    return (
      <View style={[s.container, { justifyContent: 'center', alignItems: 'center' }]}>
        <Text style={s.muted}>Seu carrinho está vazio</Text>
      </View>
    );
  }

  return (
    <View style={s.container}>
      <FlatList
        data={itens}
        keyExtractor={(i) => i.produto.id}
        renderItem={({ item }) => (
          <View style={s.linha}>
            <View style={{ flex: 1 }}>
              <Text style={s.nome}>{item.produto.nome}</Text>
              <Text style={s.muted}>{item.qtd} x {brl(item.produto.preco)}</Text>
            </View>
            <View style={s.qtdBox}>
              <TouchableOpacity style={s.qtdBtn} onPress={() => alterarQtd(item.produto.id, -1)}>
                <Text style={s.qtdTxt}>−</Text>
              </TouchableOpacity>
              <Text style={s.qtd}>{item.qtd}</Text>
              <TouchableOpacity style={s.qtdBtn} onPress={() => alterarQtd(item.produto.id, 1)}>
                <Text style={s.qtdTxt}>+</Text>
              </TouchableOpacity>
            </View>
            <Text style={s.valor}>{brl(item.produto.preco * item.qtd)}</Text>
          </View>
        )}
      />

      <View style={s.resumo}>
        <View style={s.linhaResumo}><Text style={s.muted}>Subtotal</Text><Text>{brl(subtotal)}</Text></View>
        <View style={s.linhaResumo}><Text style={s.muted}>Frete</Text><Text>{brl(frete)}</Text></View>
        <View style={s.linhaResumo}><Text style={s.total}>Total</Text><Text style={s.total}>{brl(total)}</Text></View>

        <TouchableOpacity style={s.botao} onPress={() => router.push('/confirmar-pedido')}>
          <Text style={s.botaoTxt}>Finalizar pedido</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  linha: { flexDirection: 'row', alignItems: 'center', paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: colors.border },
  nome: { fontWeight: '600', color: colors.text },
  muted: { color: colors.muted },
  qtdBox: { flexDirection: 'row', alignItems: 'center', marginHorizontal: 12 },
  qtdBtn: { width: 28, height: 28, borderRadius: 6, borderWidth: 1, borderColor: colors.border, alignItems: 'center', justifyContent: 'center' },
  qtdTxt: { fontSize: 16 },
  qtd: { marginHorizontal: 8, minWidth: 16, textAlign: 'center' },
  valor: { width: 80, textAlign: 'right', fontWeight: '600' },
  resumo: { paddingTop: 12 },
  linhaResumo: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 6 },
  total: { fontSize: 16, fontWeight: '700' },
  botao: { backgroundColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 12 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});