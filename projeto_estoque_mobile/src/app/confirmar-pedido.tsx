import { Ionicons } from '@expo/vector-icons';
import NetInfo from '@react-native-community/netinfo';
import { useRouter } from 'expo-router';
import { useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { RetiradaLoja } from '@/components/retirada-loja';
import { ScreenLayout } from '@/components/screen-layout';
import { useCart } from '@/contexts/CartContext';
import { usePedidos } from '@/contexts/PedidosContext';
import { useEmpresa } from '@/hooks/use-empresa';
import { mensagemDeErro } from '@/services/api';
import { colors } from '@/theme';
import { brl } from '@/utils/formato';

export default function ConfirmarPedido() {
  const router = useRouter();
  const { itens, subtotal, total, limpar } = useCart();
  const { criarPedido } = usePedidos();
  const { empresa, carregando: carregandoLoja } = useEmpresa();

  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  async function finalizar() {
    if (enviando) return;

    if (itens.length === 0) {
      setErro('Seu carrinho está vazio.');
      return;
    }

    setEnviando(true);
    setErro(null);

    try {
      const rede = await NetInfo.fetch();

      if (!rede.isConnected) {
        throw new Error(
          'É necessário estar conectado à internet para finalizar o pedido. Verifique sua conexão e tente novamente.'
        );
      }

      const pedido = await criarPedido(
        itens.map((item) => ({
          produtoId: item.produto.id,
          quantidade: item.qtd,
        })),
        'Retirada na loja. Pagamento: Pix.'
      );

      limpar();
      router.replace(`/pedido/${pedido.id}`);
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <ScreenLayout title="Confirmar pedido" subtitle="Revise antes de finalizar" mostrarVoltar>
      <ScrollView
        style={s.container}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}>
        <Text style={s.secao}>Retirada na loja</Text>
        <RetiradaLoja empresa={empresa} carregando={carregandoLoja} />

        <Text style={s.secao}>Forma de pagamento</Text>
        <View style={[s.card, s.linha]}>
          <Ionicons name="qr-code-outline" size={20} color={colors.primary} />
          <Text style={[s.texto, { flex: 1, marginLeft: 8 }]}>Pix</Text>
          <Ionicons name="chevron-forward" size={18} color={colors.muted} />
        </View>

        <Text style={s.secao}>Resumo do pedido</Text>
        <View style={s.card}>
          <View style={s.linha}>
            <Text style={s.muted}>Produtos</Text>
            <Text style={s.direita}>{brl(subtotal)}</Text>
          </View>
          <View style={s.linha}>
            <Text style={s.total}>Total</Text>
            <Text style={[s.total, s.direita]}>{brl(total)}</Text>
          </View>
        </View>

        <View style={s.aviso}>
          <Ionicons name="information-circle-outline" size={18} color={colors.primary} />
          <Text style={s.avisoTxt}>
            O pedido será registrado na sua conta e o estoque será reservado pelo servidor.
            A retirada será feita na loja. É necessário estar online para confirmar.
          </Text>
        </View>

        {erro ? (
          <View style={s.erroBox}>
            <Ionicons name="alert-circle" size={18} color={colors.danger} />
            <Text style={s.erroTxt}>{erro}</Text>
          </View>
        ) : null}

        <TouchableOpacity
          style={[s.botao, enviando && s.botaoDesabilitado]}
          activeOpacity={0.85}
          disabled={enviando}
          onPress={() => void finalizar()}>
          {enviando ? (
            <ActivityIndicator color="#fff" size="small" />
          ) : (
            <Text style={s.botaoTxt}>Finalizar pedido</Text>
          )}
        </TouchableOpacity>
      </ScrollView>
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  secao: { fontWeight: '700', marginTop: 12, marginBottom: 8, color: colors.text },
  card: { backgroundColor: colors.card, borderRadius: 10, padding: 12 },
  linha: { flexDirection: 'row', alignItems: 'center', marginBottom: 4 },
  texto: { color: colors.text },
  muted: { color: colors.muted, flex: 1 },
  direita: { textAlign: 'right' },
  total: { fontWeight: '700', flex: 1, fontSize: 16, color: colors.text },
  aviso: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 8,
    backgroundColor: colors.tabBg,
    borderRadius: 10,
    padding: 12,
    marginTop: 16,
  },
  avisoTxt: { flex: 1, color: colors.tabInactive, fontSize: 12, lineHeight: 17 },
  erroBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: colors.dangerBg,
    borderRadius: 10,
    padding: 12,
    marginTop: 12,
  },
  erroTxt: { flex: 1, color: colors.danger, fontSize: 12, fontWeight: '600' },
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 24,
  },
  botaoDesabilitado: { opacity: 0.7 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
});
