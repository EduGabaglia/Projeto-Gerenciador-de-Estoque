import { View, Text, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { produtos } from '@/data/mock';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';

export default function Categoria() {
  const { nome } = useLocalSearchParams<{ nome: string }>();
  const router = useRouter();
  const { add } = useCart();

  const lista = produtos.filter((p) => p.categoria === nome);

  return (
    <View style={s.container}>
      <View style={s.header}>
        <TouchableOpacity onPress={() => router.back()}>
          <Ionicons name="arrow-back" size={24} color={colors.text} />
        </TouchableOpacity>
        <Text style={s.titulo}>{nome}</Text>
      </View>

      <FlatList
        data={lista}
        keyExtractor={(p) => p.id}
        ListEmptyComponent={<Text style={s.muted}>Nenhum produto nesta categoria.</Text>}
        renderItem={({ item }) => (
          <View style={s.linha}>
            <View style={{ flex: 1 }}>
              <Text style={s.nome}>{item.nome}</Text>
              <Text style={s.muted}>R$ {item.preco.toFixed(2).replace('.', ',')}</Text>
            </View>
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
  header: { flexDirection: 'row', alignItems: 'center', gap: 12, marginBottom: 16 },
  titulo: { fontSize: 18, fontWeight: '700', color: colors.text },
  linha: { flexDirection: 'row', alignItems: 'center', backgroundColor: colors.card, borderRadius: 10, padding: 12, marginBottom: 10 },
  nome: { fontWeight: '600', color: colors.text },
  muted: { color: colors.muted },
  add: { backgroundColor: colors.primary, borderRadius: 6, padding: 6 },
});