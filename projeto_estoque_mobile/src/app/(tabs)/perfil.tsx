import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

export default function Perfil() {
  const router = useRouter();
  const { signOut } = useAuth();

  return (
    <View style={s.container}>
      <Text style={s.titulo}>Perfil</Text>

      <TouchableOpacity style={s.linha} onPress={() => router.push('/pedido/1024')}>
        <Ionicons name="receipt-outline" size={22} color={colors.primary} />
        <Text style={s.nome}>Acompanhar pedido</Text>
        <Ionicons name="chevron-forward" size={18} color={colors.muted} />
      </TouchableOpacity>

      <TouchableOpacity style={s.sair} onPress={signOut}>
        <Text style={s.sairTxt}>Sair</Text>
      </TouchableOpacity>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  titulo: { fontSize: 18, fontWeight: '700', marginBottom: 16, color: colors.text },
  linha: { flexDirection: 'row', alignItems: 'center', backgroundColor: colors.card, borderRadius: 10, padding: 14, marginBottom: 10 },
  nome: { flex: 1, marginLeft: 12, fontWeight: '600', color: colors.text },
  sair: { borderWidth: 1, borderColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 24 },
  sairTxt: { color: colors.primary, fontWeight: '600' },
});