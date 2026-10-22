import { FlatList, StyleSheet, Text, View } from 'react-native';
import { useLocalSearchParams } from 'expo-router';
import { produtos } from '@/data/mock';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';
import { ScreenLayout } from '@/components/screen-layout';
import { AddToCartButton } from '@/components/add-to-cart-button';

export default function Categoria() {
  const { nome } = useLocalSearchParams<{ nome: string }>();
  const { add } = useCart();

  const lista = produtos.filter((p) => p.categoria === nome);

  return (
    <ScreenLayout
      title={nome ?? ''}
      subtitle={`${lista.length} produtos`}
      mostrarVoltar
      voltarPara="/(tabs)/categorias">
      <FlatList
        style={s.container}
        data={lista}
        keyExtractor={(p) => p.id}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={<Text style={s.muted}>Nenhum produto nesta categoria.</Text>}
        renderItem={({ item }) => (
          <View style={s.linha}>
            <View style={{ flex: 1, minWidth: 0 }}>
              <Text style={s.nome} numberOfLines={2}>
                {item.nome}
              </Text>
              <Text style={s.muted}>R$ {item.preco.toFixed(2).replace('.', ',')}</Text>
            </View>

            <AddToCartButton onPress={() => add(item)} size={36} />
          </View>
        )}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  linha: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.card,
    borderRadius: 10,
    padding: 12,
    marginBottom: 10,
  },
  nome: { fontWeight: '600', color: colors.text },
  muted: { color: colors.muted },
});
