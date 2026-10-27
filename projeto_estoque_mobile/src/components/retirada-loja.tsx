import { Ionicons } from '@expo/vector-icons';
import { StyleSheet, Text, View } from 'react-native';
import type { Empresa } from '@/services/configuracoes';
import { colors } from '@/theme';

function nomeLoja(empresa: Empresa | null) {
  if (!empresa?.preenchido) return null;

  return empresa.nomeFantasia?.trim() || empresa.nome.trim() || null;
}

function enderecoLoja(empresa: Empresa | null) {
  if (!empresa?.preenchido) return null;

  const rua = [empresa.endereco, empresa.numero].filter(Boolean).join(', ');
  const cidade = [empresa.cidade, empresa.uf].filter(Boolean).join('/');

  return [rua, empresa.bairro, cidade, empresa.cep].filter(Boolean).join(' - ') || null;
}

type Props = {
  empresa: Empresa | null;
  carregando?: boolean;
};

/** Cartão com o local onde o cliente deve retirar o pedido. */
export function RetiradaLoja({ empresa, carregando = false }: Props) {
  const nome = nomeLoja(empresa);
  const endereco = enderecoLoja(empresa);

  return (
    <View style={s.card}>
      <View style={s.iconeBox}>
        <Ionicons name="storefront-outline" size={20} color={colors.primary} />
      </View>

      <View style={s.textoBox}>
        {carregando ? (
          <>
            <Text style={s.titulo}>Retirada na loja</Text>
            <Text style={s.detalhe}>Carregando endereço da loja...</Text>
          </>
        ) : (
          <>
            <Text style={s.titulo}>{nome ?? 'Retirada na loja'}</Text>
            <Text style={s.detalhe}>
              {endereco ?? 'Consulte a loja para confirmar o endereço.'}
            </Text>
          </>
        )}
      </View>
    </View>
  );
}

const s = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 10,
    backgroundColor: colors.card,
    borderRadius: 10,
    padding: 12,
  },
  iconeBox: {
    width: 34,
    height: 34,
    borderRadius: 8,
    backgroundColor: colors.tabBg,
    alignItems: 'center',
    justifyContent: 'center',
  },
  textoBox: { flex: 1, paddingTop: 1 },
  titulo: { color: colors.text, fontWeight: '700', fontSize: 14 },
  detalhe: { color: colors.muted, fontSize: 12, lineHeight: 17, marginTop: 2 },
});
