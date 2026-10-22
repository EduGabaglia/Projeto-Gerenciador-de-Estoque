import { useEffect, useRef, useState } from 'react';
import {
  Animated,
  FlatList,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useCart, type Item } from '@/contexts/CartContext';
import { colors } from '@/theme';
import { ScreenLayout } from '@/components/screen-layout';

const brl = (v: number) => 'R$ ' + v.toFixed(2).replace('.', ',');

/** Linha do carrinho com pulso quando a quantidade muda. */
function LinhaCarrinho({ item }: { item: Item }) {
  const { alterarQtd } = useCart();
  const [escala] = useState(() => new Animated.Value(1));
  const qtdAnterior = useRef(item.qtd);

  useEffect(() => {
    if (item.qtd === qtdAnterior.current) return;

    qtdAnterior.current = item.qtd;
    escala.stopAnimation();
    escala.setValue(0.94);

    Animated.spring(escala, {
      toValue: 1,
      speed: 18,
      bounciness: 12,
      useNativeDriver: true,
    }).start();
  }, [item.qtd, escala]);

  return (
    <Animated.View style={[s.linha, { transform: [{ scale: escala }] }]}>
      <View style={{ flex: 1, minWidth: 0 }}>
        <Text style={s.nome} numberOfLines={2}>
          {item.produto.nome}
        </Text>
        <Text style={s.muted}>
          {item.qtd} x {brl(item.produto.preco)}
        </Text>
      </View>

      <View style={s.qtdBox}>
        <TouchableOpacity
          style={s.qtdBtn}
          onPress={() => alterarQtd(item.produto.id, -1)}
          hitSlop={6}>
          <Text style={s.qtdTxt}>−</Text>
        </TouchableOpacity>

        <Text style={s.qtd}>{item.qtd}</Text>

        <TouchableOpacity
          style={s.qtdBtn}
          onPress={() => alterarQtd(item.produto.id, 1)}
          hitSlop={6}>
          <Text style={s.qtdTxt}>+</Text>
        </TouchableOpacity>
      </View>

      <Text style={s.valor}>{brl(item.produto.preco * item.qtd)}</Text>
    </Animated.View>
  );
}

/** Total do carrinho com pulso a cada alteração. */
function TotalAnimado({ valor }: { valor: number }) {
  const [escala] = useState(() => new Animated.Value(1));
  const valorAnterior = useRef(valor);

  useEffect(() => {
    if (valor === valorAnterior.current) return;

    valorAnterior.current = valor;
    escala.stopAnimation();
    escala.setValue(1.12);

    Animated.spring(escala, {
      toValue: 1,
      speed: 18,
      bounciness: 10,
      useNativeDriver: true,
    }).start();
  }, [valor, escala]);

  return (
    <Animated.Text style={[s.total, { transform: [{ scale: escala }] }]}>{brl(valor)}</Animated.Text>
  );
}

export default function Carrinho() {
  const router = useRouter();
  const { itens, subtotal, frete, total } = useCart();

  return (
    <ScreenLayout title="Carrinho" subtitle={`${itens.length} itens adicionados`} mostrarCarrinho={false}>
      {itens.length === 0 ? (
        <View style={[s.container, s.vazio]}>
          <Text style={s.muted}>Seu carrinho está vazio</Text>
        </View>
      ) : (
        <View style={s.container}>
          <FlatList
            data={itens}
            keyExtractor={(i) => i.produto.id}
            contentContainerStyle={s.lista}
            showsVerticalScrollIndicator={false}
            renderItem={({ item }) => <LinhaCarrinho item={item} />}
          />

          <View style={s.resumo}>
            <View style={s.linhaResumo}>
              <Text style={s.muted}>Subtotal</Text>
              <Text>{brl(subtotal)}</Text>
            </View>
            <View style={s.linhaResumo}>
              <Text style={s.muted}>Frete</Text>
              <Text>{brl(frete)}</Text>
            </View>
            <View style={s.linhaResumo}>
              <Text style={s.total}>Total</Text>
              <TotalAnimado valor={total} />
            </View>

            <TouchableOpacity
              style={s.botao}
              activeOpacity={0.85}
              onPress={() => router.push('/confirmar-pedido')}>
              <Text style={s.botaoTxt}>Finalizar pedido</Text>
            </TouchableOpacity>
          </View>
        </View>
      )}
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  vazio: { alignItems: 'center', justifyContent: 'center', padding: 24 },
  lista: { padding: 16, paddingBottom: 8 },
  linha: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  nome: { fontWeight: '600', color: colors.text },
  muted: { color: colors.muted },
  qtdBox: { flexDirection: 'row', alignItems: 'center', marginHorizontal: 12 },
  qtdBtn: {
    width: 28,
    height: 28,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtdTxt: { fontSize: 16, color: colors.text },
  qtd: { marginHorizontal: 8, minWidth: 16, textAlign: 'center', color: colors.text },
  valor: { width: 80, textAlign: 'right', fontWeight: '600', color: colors.text },
  resumo: { padding: 16, borderTopWidth: 1, borderTopColor: colors.border },
  linhaResumo: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 6,
    alignItems: 'center',
  },
  total: { fontSize: 16, fontWeight: '700', color: colors.text },
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 12,
  },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});
