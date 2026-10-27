import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuth } from '@/contexts/AuthContext';
import { usePedidos } from '@/contexts/PedidosContext';
import { colors } from '@/theme';

export default function Perfil() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { signOut, usuario } = useAuth();
  const { pedidos } = usePedidos();

  return (
    <ScrollView
      style={s.container}
      contentContainerStyle={[s.conteudo, { paddingTop: insets.top + 16 }]}
      showsVerticalScrollIndicator={false}>
      <Text style={s.titulo}>Perfil</Text>

      <View style={s.usuarioCard}>
        <View style={s.avatar}>
          <Ionicons name="person" size={26} color={colors.primary} />
        </View>
        <View style={s.usuarioInfo}>
          <Text style={s.usuarioNome} numberOfLines={1}>
            {usuario?.nome ?? 'Cliente'}
          </Text>
          <Text style={s.usuarioEmail} numberOfLines={1}>
            {usuario?.email ?? ''}
          </Text>
        </View>
      </View>

      <TouchableOpacity style={s.linha} activeOpacity={0.8} onPress={() => router.push('/compras')}>
        <View style={[s.icone, { backgroundColor: colors.tabBg }]}>
          <Ionicons name="receipt-outline" size={20} color={colors.primary} />
        </View>
        <View style={s.linhaTexto}>
          <Text style={s.linhaTitulo}>Minhas compras</Text>
          <Text style={s.linhaSub}>
            {pedidos.length} {pedidos.length === 1 ? 'pedido' : 'pedidos'} no histórico
          </Text>
        </View>
        <Ionicons name="chevron-forward" size={18} color={colors.muted} />
      </TouchableOpacity>

      <TouchableOpacity style={s.linha} activeOpacity={0.8} onPress={() => router.push('/conta')}>
        <View style={[s.icone, { backgroundColor: colors.tabBg }]}>
          <Ionicons name="settings-outline" size={20} color={colors.primary} />
        </View>
        <View style={s.linhaTexto}>
          <Text style={s.linhaTitulo}>Gerenciar conta</Text>
          <Text style={s.linhaSub}>Nome, senha e exclusão da conta</Text>
        </View>
        <Ionicons name="chevron-forward" size={18} color={colors.muted} />
      </TouchableOpacity>

      <TouchableOpacity style={s.sair} activeOpacity={0.85} onPress={signOut}>
        <Ionicons name="log-out-outline" size={18} color={colors.primary} />
        <Text style={s.sairTxt}>Sair</Text>
      </TouchableOpacity>
    </ScrollView>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  conteudo: { padding: 16, paddingBottom: 32 },
  titulo: { fontSize: 18, fontWeight: '700', marginBottom: 16, color: colors.text },

  usuarioCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 14,
    marginBottom: 18,
  },
  avatar: {
    width: 48,
    height: 48,
    borderRadius: 24,
    backgroundColor: colors.tabBg,
    alignItems: 'center',
    justifyContent: 'center',
  },
  usuarioInfo: { flex: 1, minWidth: 0 },
  usuarioNome: { fontSize: 16, fontWeight: '700', color: colors.text },
  usuarioEmail: { fontSize: 12, color: colors.muted, marginTop: 2 },

  linha: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 14,
    marginBottom: 10,
  },
  icone: {
    width: 38,
    height: 38,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
  },
  linhaTexto: { flex: 1, minWidth: 0 },
  linhaTitulo: { fontWeight: '600', color: colors.text },
  linhaSub: { fontSize: 11, color: colors.muted, marginTop: 1 },

  sair: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    borderWidth: 1,
    borderColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    marginTop: 12,
  },
  sairTxt: { color: colors.primary, fontWeight: '700' },
});
