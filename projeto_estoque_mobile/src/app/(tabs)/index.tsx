import { View, Text, TextInput, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { categorias, produtos } from '@/data/mock';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';

export default function Inicio() {
  const router = useRouter();
  const { add } = useCart();

  return (
    <View style={s.container}>
      <TextInput style={s.busca} placeholder="Buscar produtos..." />

      <View style={s.categorias}>
        {categorias.map((c) => (
          <TouchableOpacity key={c} style={s.cat} onPress={() => router.push(`/categoria/${c}`)}>
            <Ionicons name="pricetag" size={26} color={colors.primary} />
            <Text style={s.catTxt}>{c}</Text>
          </TouchableOpacity>
        ))}
      </View>

      <Text style={s.secao}>Em destaque</Text>
      <FlatList
        data={produtos.filter((p) => p.destaque)}
        keyExtractor={(p) => p.id}
        numColumns={2}
        columnWrapperStyle={{ gap: 12 }}
        renderItem={({ item }) => (
          <View style={s.card}>
            <Text style={s.nome}>{item.nome}</Text>
            <Text style={s.preco}>R$ {item.preco.toFixed(2).replace('.', ',')}</Text>
            <TouchableOpacity style={s.add} onPress={() => add(item)}>
              <Ionicons name="add" size={20} color="#fff" />
            </TouchableOpacity>
          </View>
        )}
      />
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  busca: { borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 12, marginBottom: 16 },
  categorias: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 20 },
  cat: { alignItems: 'center', backgroundColor: colors.card, padding: 12, borderRadius: 10, width: 76 },
  catTxt: { fontSize: 11, marginTop: 4 },
  secao: { fontSize: 16, fontWeight: '700', marginBottom: 12 },
  card: { flex: 1, backgroundColor: colors.card, borderRadius: 10, padding: 12, marginBottom: 12, height: 130, justifyContent: 'space-between' },
  nome: { fontWeight: '600' },
  preco: { color: colors.muted },
  add: { alignSelf: 'flex-end', backgroundColor: colors.primary, borderRadius: 6, padding: 4 },
});