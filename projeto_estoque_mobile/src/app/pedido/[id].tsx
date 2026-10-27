import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { ConfirmDialog } from '@/components/confirm-dialog';
import { RetiradaLoja } from '@/components/retirada-loja';
import { ScreenLayout } from '@/components/screen-layout';
import { StatusPedidoChip } from '@/components/status-pedido';
import { usePedidos } from '@/contexts/PedidosContext';
import { STATUS_LABEL, type Pedido } from '@/data/pedidos';
import { useEmpresa } from '@/hooks/use-empresa';
import { mensagemDeErro } from '@/services/api';
import { colors } from '@/theme';
import { brl, formatarData } from '@/utils/formato';

const etapas = ['Pedido realizado', 'Preparando', 'Pronto para retirada', 'Retirado'];

function etapaDoStatus(status: Pedido['status']) {
  switch (status) {
    case 'PROCESSANDO':
      return 1;
    case 'PRONTO_PARA_RETIRADA':
      return 2;
    case 'RETIRADO':
      return 3;
    default:
      return 0;
  }
}

export default function Acompanhamento() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const { obterPedido, carregarPedido, cancelarPedido } = usePedidos();
  const { empresa, carregando: carregandoLoja } = useEmpresa();

  const pedidoId = Number(id);
  const idValido = Number.isInteger(pedidoId) && pedidoId > 0;

  const [pedido, setPedido] = useState<Pedido | null>(() =>
    idValido ? (obterPedido(pedidoId) ?? null) : null
  );
  const [carregando, setCarregando] = useState(idValido && !pedido);
  const [erro, setErro] = useState<string | null>(
    idValido ? null : 'Pedido inválido.'
  );
  const [tentativa, setTentativa] = useState(0);

  const [confirmando, setConfirmando] = useState(false);
  const [cancelando, setCancelando] = useState(false);
  const [erroCancelamento, setErroCancelamento] = useState<string | null>(null);

  useEffect(() => {
    if (!idValido) return;

    let ativo = true;

    carregarPedido(pedidoId)
      .then((encontrado) => {
        if (!ativo) return;
        setPedido(encontrado);
        setErro(null);
      })
      .catch((falha) => {
        if (!ativo) return;
        setErro(mensagemDeErro(falha));
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [pedidoId, idValido, carregarPedido, tentativa]);

  function tentarNovamente() {
    setErro(null);
    setCarregando(true);
    setTentativa((valor) => valor + 1);
  }

  async function confirmarCancelamento() {
    if (!pedido) return;

    setCancelando(true);
    setErroCancelamento(null);

    try {
      const atualizado = await cancelarPedido(pedido.id);
      setPedido(atualizado);
      setConfirmando(false);
    } catch (falha) {
      setErroCancelamento(mensagemDeErro(falha));
      setConfirmando(false);
    } finally {
      setCancelando(false);
    }
  }

  if (carregando && !pedido) {
    return (
      <ScreenLayout title="Pedido" subtitle="Carregando" mostrarVoltar voltarPara="/compras">
        <View style={s.vazio}>
          <ActivityIndicator color={colors.primary} size="large" />
        </View>
      </ScreenLayout>
    );
  }

  if (!pedido) {
    return (
      <ScreenLayout title="Pedido" subtitle="Não encontrado" mostrarVoltar voltarPara="/compras">
        <View style={s.vazio}>
          <Ionicons name="receipt-outline" size={48} color={colors.muted} />
          <Text style={s.vazioTitulo}>Pedido não encontrado</Text>
          <Text style={s.vazioTexto}>
            {erro ?? 'Ele pode ter sido removido ou o link está incorreto.'}
          </Text>

          <TouchableOpacity
            style={[s.botao, { alignSelf: 'stretch' }]}
            activeOpacity={0.85}
            onPress={tentarNovamente}>
            <Text style={s.botaoTxt}>Tentar novamente</Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[s.botaoSecundario, { alignSelf: 'stretch' }]}
            activeOpacity={0.85}
            onPress={() => router.replace('/compras')}>
            <Text style={s.botaoSecundarioTxt}>Ir para minhas compras</Text>
          </TouchableOpacity>
        </View>
      </ScreenLayout>
    );
  }

  const cancelado = pedido.status === 'CANCELADO';
  const etapaAtual = etapaDoStatus(pedido.status);
  const podeCancelar = pedido.status === 'PENDENTE';

  return (
    <ScreenLayout
      title={`Pedido #${pedido.numero}`}
      subtitle={STATUS_LABEL[pedido.status]}
      mostrarVoltar
      voltarPara="/compras">
      <ScrollView
        style={s.container}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}>
        {erro ? (
          <View style={s.avisoBox}>
            <Ionicons name="cloud-offline-outline" size={18} color={colors.warning} />
            <Text style={s.avisoTxt}>{erro}</Text>
          </View>
        ) : null}

        {cancelado ? (
          <View style={s.banner}>
            <Ionicons name="close-circle" size={22} color={colors.danger} />
            <View style={s.bannerTexto}>
              <Text style={s.bannerTitulo}>Pedido cancelado</Text>
              <Text style={s.bannerMsg}>
                Este pedido foi cancelado e não estará disponível para retirada. Se você
                não reconhece o cancelamento, fale com o suporte.
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

        {!cancelado ? (
          <>
            <Text style={s.secao}>Retirada na loja</Text>
            <RetiradaLoja empresa={empresa} carregando={carregandoLoja} />
          </>
        ) : null}

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

        {pedido.observacao ? (
          <View style={s.observacao}>
            <Ionicons name="information-circle-outline" size={16} color={colors.muted} />
            <Text style={s.observacaoTxt}>{pedido.observacao}</Text>
          </View>
        ) : null}

        {erroCancelamento ? (
          <View style={s.erroBox}>
            <Ionicons name="alert-circle" size={17} color={colors.danger} />
            <Text style={s.erroTxt}>{erroCancelamento}</Text>
          </View>
        ) : null}

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

  avisoBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: colors.warningBg,
    borderRadius: 10,
    padding: 10,
    marginBottom: 12,
  },
  avisoTxt: { color: colors.warning, fontSize: 12, flex: 1 },

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

  observacao: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 6,
    marginTop: 10,
  },
  observacaoTxt: { color: colors.muted, fontSize: 11, flex: 1, lineHeight: 16 },

  erroBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: colors.dangerBg,
    borderRadius: 10,
    padding: 12,
    marginTop: 16,
  },
  erroTxt: { color: colors.danger, fontSize: 12, flex: 1 },

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

  botaoSecundario: {
    borderWidth: 1,
    borderColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 10,
  },
  botaoSecundarioTxt: { color: colors.primary, fontWeight: '700' },

  vazio: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, gap: 8 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text },
  vazioTexto: { color: colors.muted, textAlign: 'center', marginBottom: 12 },
});
