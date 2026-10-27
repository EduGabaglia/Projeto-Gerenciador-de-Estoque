import { Ionicons } from '@expo/vector-icons';
import { useEffect, useRef, useState } from 'react';
import { Animated, Easing, StyleSheet, Text, View, type ColorValue } from 'react-native';
import { useCart } from '@/contexts/CartContext';
import { colors } from '@/theme';

type Props = {
  color?: ColorValue;
  size?: number;
  /** Exibe a quantidade de itens no badge. */
  showBadge?: boolean;
};

/**
 * Ícone do carrinho com animação de "pulo" disparada sempre que
 * um item é adicionado ao carrinho.
 */
export function AnimatedCartIcon({ color = colors.primary, size = 24, showBadge = true }: Props) {
  const { totalItens, adicoes } = useCart();

  const [escala] = useState(() => new Animated.Value(1));
  const [escalaBadge] = useState(() => new Animated.Value(1));
  const totalAnterior = useRef(totalItens);
  const adicoesAnterior = useRef(adicoes);

  // Pulso do ícone a cada item adicionado.
  useEffect(() => {
    if (adicoes === adicoesAnterior.current) return;

    adicoesAnterior.current = adicoes;
    escala.stopAnimation();
    escala.setValue(1);

    Animated.sequence([
      Animated.timing(escala, {
        toValue: 1.4,
        duration: 150,
        easing: Easing.out(Easing.quad),
        useNativeDriver: true,
      }),
      Animated.spring(escala, {
        toValue: 1,
        speed: 16,
        bounciness: 12,
        useNativeDriver: true,
      }),
    ]).start();
  }, [adicoes, escala]);

  // Entrada/saída suave do badge conforme a quantidade muda.
  useEffect(() => {
    if (totalItens === totalAnterior.current) return;

    totalAnterior.current = totalItens;

    if (totalItens === 0) {
      Animated.timing(escalaBadge, {
        toValue: 0.6,
        duration: 120,
        easing: Easing.out(Easing.quad),
        useNativeDriver: true,
      }).start();
      return;
    }

    escalaBadge.setValue(0.6);
    Animated.spring(escalaBadge, {
      toValue: 1,
      speed: 20,
      bounciness: 14,
      useNativeDriver: true,
    }).start();
  }, [totalItens, escalaBadge]);

  const mostrarBadge = showBadge && totalItens > 0;

  return (
    <View style={[s.container, { width: size + 10, height: size + 6 }]}>
      <Animated.View style={{ transform: [{ scale: escala }] }}>
        <Ionicons name="cart" size={size} color={color} />
      </Animated.View>

      {mostrarBadge ? (
        <Animated.View style={[s.badge, { transform: [{ scale: escalaBadge }] }]}>
          <Text style={s.badgeTexto} numberOfLines={1}>
            {totalItens > 99 ? '99+' : totalItens}
          </Text>
        </Animated.View>
      ) : null}
    </View>
  );
}

const s = StyleSheet.create({
  container: { alignItems: 'center', justifyContent: 'center' },
  badge: {
    position: 'absolute',
    top: -4,
    right: -4,
    minWidth: 17,
    height: 17,
    paddingHorizontal: 4,
    borderRadius: 9,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeTexto: { color: '#fff', fontSize: 10, fontWeight: '700' },
});
