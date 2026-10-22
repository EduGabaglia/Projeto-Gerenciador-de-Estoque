import { FlatList, ScrollView, StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { categorias, produtos } from '@/data/mock';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';
import { ScreenLayout } from '@/components/screen-layout';
import { AddToCartButton } from '@/components/add-to-cart-button';
import { useGrade } from '@/hooks/use-grade';

export default function Inicio() {
  const router = useRouter();
  const { add } = useCart();
  const { colunas } = useGrade();

  return (
    <ScreenLayout title="Início" subtitle="Escolha seus produtos favoritos">
      <View style={s.container}>
        <TextInput
          style={s.busca}
          placeholder="Buscar produtos..."
          placeholderTextColor={colors.muted}
        />

        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={s.categorias}>
          {categorias.map((c) => (
            <TouchableOpacity
              key={c}
              style={s.cat}
              onPress={() => router.push(`/categoria/${c}`)}>
              <Ionicons name="pricetag" size={24} color={colors.primary} />
              <Text style={s.catTxt} numberOfLines={1}>
                {c}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        <Text style={s.secao}>Em destaque</Text>

        <FlatList
          key={`grade-${colunas}`}
          data={produtos.filter((p) => p.destaque)}
          keyExtractor={(p) => p.id}
          numColumns={colunas}
          columnWrapperStyle={{ gap: 12 }}
          contentContainerStyle={s.lista}
          showsVerticalScrollIndicator={false}
          renderItem={({ item }) => (
            <View style={s.card}>
              <Text style={s.nome} numberOfLines={2}>
                {item.nome}
              </Text>
              <Text style={s.preco}>R$ {item.preco.toFixed(2).replace('.', ',')}</Text>
              <AddToCartButton onPress={() => add(item)} />
            </View>
          )}
        />
      </View>
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16 },
  busca: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    padding: 12,
    marginBottom: 16,
    color: colors.text,
  },
  categorias: { gap: 10, paddingBottom: 4 },
  cat: {
    alignItems: 'center',
    backgroundColor: colors.card,
    paddingVertical: 12,
    paddingHorizontal: 14,
    borderRadius: 10,
    width: 92,
  },
  catTxt: { fontSize: 11, marginTop: 4, color: colors.text },
  secao: { fontSize: 16, fontWeight: '700', marginBottom: 12, marginTop: 16, color: colors.text },
  lista: { paddingBottom: 24 },
  card: {
    flex: 1,
    backgroundColor: colors.card,
    borderRadius: 10,
    padding: 12,
    marginBottom: 12,
    minHeight: 120,
    justifyContent: 'space-between',
  },
  nome: { fontWeight: '600', color: colors.text },
  preco: { color: colors.muted, marginTop: 4 },
});
