import { Ionicons } from '@expo/vector-icons';
import type { ComponentProps } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import type { StatusPedido } from '@/data/pedidos';
import { STATUS_LABEL } from '@/data/pedidos';
import { colors } from '@/theme';

const META: Record<
  StatusPedido,
  { cor: string; fundo: string; icone: ComponentProps<typeof Ionicons>['name'] }
> = {
  andamento: { cor: colors.warning, fundo: colors.warningBg, icone: 'time-outline' },
  entregue: { cor: colors.success, fundo: colors.successBg, icone: 'checkmark-circle-outline' },
  cancelado: { cor: colors.danger, fundo: colors.dangerBg, icone: 'close-circle-outline' },
};

/** Etiqueta colorida com o status do pedido. */
export function StatusPedidoChip({ status }: { status: StatusPedido }) {
  const meta = META[status];

  return (
    <View style={[s.chip, { backgroundColor: meta.fundo }]}>
      <Ionicons name={meta.icone} size={13} color={meta.cor} />
      <Text style={[s.texto, { color: meta.cor }]}>{STATUS_LABEL[status]}</Text>
    </View>
  );
}

const s = StyleSheet.create({
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    alignSelf: 'flex-start',
    paddingVertical: 3,
    paddingHorizontal: 8,
    borderRadius: 999,
  },
  texto: { fontSize: 11, fontWeight: '700' },
});
