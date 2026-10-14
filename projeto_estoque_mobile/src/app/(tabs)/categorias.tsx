import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { categorias } from '@/data/mock';
import { colors } from '@/theme';

export default function Categorias() {
  const router = useRouter();

  return (
    <View style={s.container}>
      <Text style={s.titulo}>Categorias</Text>
      {categorias.map((c) => (
        <TouchableOpacity key={c} style={s.linha} onPress={() => router.push(`/categoria/${c}`)}>
          <Ionicons name="pricetag" size={22} color={colors.primary} />
          <Text style={s.nome}>{c}</Text>
          <Ionicons name="chevron-forward" size={18} color={colors.muted} />
        </TouchableOpacity>
      ))}
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  titulo: { fontSize: 18, fontWeight: '700', marginBottom: 16, color: colors.text },
  linha: { flexDirection: 'row', alignItems: 'center', backgroundColor: colors.card, borderRadius: 10, padding: 14, marginBottom: 10 },
  nome: { flex: 1, marginLeft: 12, fontWeight: '600', color: colors.text },
});