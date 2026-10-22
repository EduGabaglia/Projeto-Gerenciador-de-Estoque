import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { ConfirmDialog } from '@/components/confirm-dialog';
import { ScreenLayout } from '@/components/screen-layout';
import { StatusPedidoChip } from '@/components/status-pedido';
import { usePedidos } from '@/contexts/PedidosContext';
import { brl, formatarData, STATUS_LABEL } from '@/data/pedidos';
import { colors } from '@/theme';

const etapas = ['Pedido realizado', 'Preparando', 'Pronto para retirada', 'Entregue'];

export default function Acompanhamento() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { obterPedido, cancelarPedido, carregando } = usePedidos();

  const [confirmando, setConfirmando] = useState(false);
  const [cancelando, setCancelando] = useState(false);

  const pedido = obterPedido(id ?? '');

  if (carregando) {
    return (
      <ScreenLayout title="Pedido" subtitle="Carregando" mostrarVoltar voltarPara="/(tabs)/perfil">
        <View style={s.vazio}>
          <ActivityIndicator color={colors.primary} size="large" />
        </View>
      </ScreenLayout>
    );
  }

  if (!pedido) {
    return (
      <ScreenLayout
        title="Pedido"
        subtitle="Não encontrado"
        mostrarVoltar
        voltarPara="/(tabs)/perfil">
        <View style={s.vazio}>
          <Ionicons name="receipt-outline" size={48} color={colors.muted} />
          <Text style={s.vazioTitulo}>Pedido não encontrado</Text>
          <Text style={s.vazioTexto}>
            Ele pode ter sido removido ou o link está incorreto.
          </Text>

          <TouchableOpacity
            style={[s.botao, { alignSelf: 'stretch' }]}
            activeOpacity={0.85}
            onPress={() => router.replace('/compras')}>
            <Text style={s.botaoTxt}>Ir para minhas compras</Text>
          </TouchableOpacity>
        </View>
      </ScreenLayout>
    );
  }

  const cancelado = pedido.status === 'cancelado';
  const entregue = pedido.status === 'entregue';
  const etapaAtual = entregue ? etapas.length - 1 : 1;
  const podeCancelar = pedido.status === 'andamento';
  const pedidoId = pedido.id;

  async function confirmarCancelamento() {
    setCancelando(true);
    try {
      await cancelarPedido(pedidoId);
    } finally {
      setCancelando(false);
      setConfirmando(false);
    }
  }

  return (
    <ScreenLayout
      title={`Pedido #${pedido.numero}`}
      subtitle={STATUS_LABEL[pedido.status]}
      mostrarVoltar
      voltarPara="/(tabs)/perfil">
      <ScrollView
        style={s.container}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}>
        {cancelado ? (
          <View style={s.banner}>
            <Ionicons name="close-circle" size={22} color={colors.danger} />
            <View style={s.bannerTexto}>
              <Text style={s.bannerTitulo}>Pedido cancelado</Text>
              <Text style={s.bannerMsg}>
                Este pedido foi cancelado e não será entregue. Se você não reconhece o
                cancelamento, fale com o suporte.
              </Text>
            </View>
          </View>
        ) : (
          <View>
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
                      <View
                        style={[
                          s.trilho,
                          { backgroundColor: i < etapaAtual ? colors.primary : colors.border },
                        ]}
                      />
                    )}
                  </View>
                  <Text style={[s.etapaNome, !feita && { color: colors.muted }]}>{nome}</Text>
                </View>
              );
            })}
          </View>
        )}

        <Text style={s.secao}>Resumo do pedido</Text>

        <View style={s.card}>
          <View style={s.linhaInfo}>
            <Text style={s.muted}>Data</Text>
            <Text style={s.valor}>{formatarData(pedido.criadoEm)}</Text>
          </View>

          <View style={s.linhaInfo}>
            <Text style={s.muted}>Status</Text>
            <StatusPedidoChip status={pedido.status} />
          </View>

          {pedido.itens.map((item) => (
            <View key={item.produtoId} style={s.linhaInfo}>
              <Text style={[s.muted, s.itemNome]} numberOfLines={1}>
                {item.qtd}x {item.nome}
              </Text>
              <Text style={s.valor}>{brl(item.preco * item.qtd)}</Text>
            </View>
          ))}

          <View style={[s.linhaInfo, s.totalLinha]}>
            <Text style={s.total}>Total</Text>
            <Text style={s.total}>{brl(pedido.total)}</Text>
          </View>
        </View>

        {podeCancelar ? (
          <TouchableOpacity
            style={s.cancelar}
            activeOpacity={0.85}
            onPress={() => setConfirmando(true)}>
            <Ionicons name="close-circle-outline" size={18} color={colors.danger} />
            <Text style={s.cancelarTxt}>Cancelar pedido</Text>
          </TouchableOpacity>
        ) : null}

        <TouchableOpacity
          style={s.botao}
          activeOpacity={0.85}
          onPress={() => router.replace('/(tabs)')}>
          <Text style={s.botaoTxt}>Voltar ao início</Text>
        </TouchableOpacity>
      </ScrollView>

      <ConfirmDialog
        visivel={confirmando}
        titulo="Cancelar pedido"
        mensagem={`Tem certeza que deseja cancelar o pedido #${pedido.numero}? Essa ação não pode ser desfeita.`}
        confirmar="Sim, cancelar"
        cancelar="Manter pedido"
        destrutivo
        carregando={cancelando}
        onConfirmar={confirmarCancelamento}
        onCancelar={() => setConfirmando(false)}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 32 },

  etapa: { flexDirection: 'row', minHeight: 56 },
  coluna: { alignItems: 'center', width: 32 },
  trilho: { width: 2, flex: 1, marginVertical: 2 },
  etapaNome: { marginLeft: 12, fontSize: 15, color: colors.text, paddingTop: 2 },

  banner: {
    flexDirection: 'row',
    gap: 10,
    backgroundColor: colors.dangerBg,
    borderRadius: 12,
    padding: 14,
  },
  bannerTexto: { flex: 1 },
  bannerTitulo: { fontWeight: '700', color: colors.danger, fontSize: 14 },
  bannerMsg: { color: colors.danger, fontSize: 12, marginTop: 2, lineHeight: 17 },

  secao: { fontSize: 14, fontWeight: '700', color: colors.text, marginTop: 20, marginBottom: 8 },
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    paddingHorizontal: 14,
    paddingVertical: 6,
  },
  linhaInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 12,
    paddingVertical: 7,
  },
  muted: { color: colors.muted, fontSize: 13 },
  itemNome: { flex: 1 },
  valor: { color: colors.text, fontSize: 13 },
  totalLinha: {
    borderTopWidth: 1,
    borderTopColor: colors.border,
    marginTop: 4,
    paddingTop: 12,
  },
  total: { fontSize: 16, fontWeight: '700', color: colors.text },

  cancelar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    borderWidth: 1,
    borderColor: colors.danger,
    backgroundColor: colors.dangerBg,
    padding: 14,
    borderRadius: 10,
    marginTop: 20,
  },
  cancelarTxt: { color: colors.danger, fontWeight: '700' },

  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 12,
  },
  botaoTxt: { color: '#fff', fontWeight: '700' },

  vazio: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, gap: 8 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text },
  vazioTexto: { color: colors.muted, textAlign: 'center', marginBottom: 12 },
});
