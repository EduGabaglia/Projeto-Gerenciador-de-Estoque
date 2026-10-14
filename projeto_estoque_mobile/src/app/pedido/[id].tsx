import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { colors } from '@/theme';

const etapas = [
  'Pedido realizado',
  'Preparando',
  'Pronto para entrega',
  'Em rota de entrega',
  'Entregue',
];

export default function Acompanhamento() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  // MOCK: etapa atual (0 a 4). Depois vem da API.
  const etapaAtual = 1;

  return (
    <View style={s.container}>
      <View style={s.header}>
        <TouchableOpacity onPress={() => router.replace('/(tabs)')}>
          <Ionicons name="arrow-back" size={24} color={colors.text} />
        </TouchableOpacity>
        <Text style={s.titulo}>Pedido #{id}</Text>
      </View>

      {etapas.map((nome, i) => {
        const feita = i <= etapaAtual;
        return (
          <View key={nome} style={s.etapa}>
            <View style={s.coluna}>
              <Ionicons
                name={feita ? 'checkmark-circle' : 'ellipse-outline'}
                size={24}
                color={feita ? colors.primary : colors.border}
              />
              {i < etapas.length - 1 && (
                <View style={[s.trilho, { backgroundColor: i < etapaAtual ? colors.primary : colors.border }]} />
              )}
            </View>
            <Text style={[s.nome, !feita && { color: colors.muted }]}>{nome}</Text>
          </View>
        );
      })}

      <TouchableOpacity style={s.botao} onPress={() => router.replace('/(tabs)')}>
        <Text style={s.botaoTxt}>Voltar ao início</Text>
      </TouchableOpacity>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16, paddingTop: 56 },
  header: { flexDirection: 'row', alignItems: 'center', gap: 12, marginBottom: 24 },
  titulo: { fontSize: 18, fontWeight: '700', color: colors.text },
  etapa: { flexDirection: 'row', minHeight: 56 },
  coluna: { alignItems: 'center', width: 32 },
  trilho: { width: 2, flex: 1, marginVertical: 2 },
  nome: { marginLeft: 12, fontSize: 15, color: colors.text, paddingTop: 2 },
  botao: { backgroundColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 24 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});