import { ScrollView, StyleSheet, Text, TouchableOpacity } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { categorias } from '@/data/mock';
import { colors } from '@/theme';
import { ScreenLayout } from '@/components/screen-layout';

export default function Categorias() {
  const router = useRouter();

  return (
    <ScreenLayout title="Categorias" subtitle="Navegue por todas as seções">
      <ScrollView
        style={s.container}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}>
        {categorias.map((c) => (
          <TouchableOpacity
            key={c}
            style={s.linha}
            onPress={() => router.push(`/categoria/${c}`)}>
            <Ionicons name="pricetag" size={22} color={colors.primary} />
            <Text style={s.nome}>{c}</Text>
            <Ionicons name="chevron-forward" size={18} color={colors.muted} />
          </TouchableOpacity>
        ))}
      </ScrollView>
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
    padding: 14,
    marginBottom: 10,
  },
  nome: { flex: 1, marginLeft: 12, fontWeight: '600', color: colors.text },
});
