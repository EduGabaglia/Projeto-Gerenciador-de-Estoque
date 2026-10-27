import { StyleSheet, Text, View } from 'react-native';
import { precoVenda, type Produto } from '@/data/catalogo';
import { brl } from '@/utils/formato';
import { colors } from '@/theme';

type Props = {
  produto: Produto;
  tamanho?: number;
};

/** Preço de venda, mostrando o valor original quando houver promoção. */
export function PrecoProduto({ produto, tamanho = 14 }: Props) {
  const promocional = produto.precoPromocional;
  const valor = precoVenda(produto);

  return (
    <View style={s.linha}>
      {promocional != null ? (
        <Text style={[s.original, { fontSize: Math.max(11, tamanho - 2) }]}>
          {brl(produto.preco)}
        </Text>
      ) : null}
      <Text
        style={[
          s.atual,
          { fontSize: tamanho },
          promocional != null && { color: colors.primary },
        ]}>
        {brl(valor)}
      </Text>
    </View>
  );
}

const s = StyleSheet.create({
  linha: { flexDirection: 'row', alignItems: 'baseline', flexWrap: 'wrap', gap: 6 },
  original: { color: colors.muted, textDecorationLine: 'line-through' },
  atual: { fontWeight: '700', color: colors.text },
});
