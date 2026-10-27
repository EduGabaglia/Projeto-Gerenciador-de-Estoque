import { Ionicons } from '@expo/vector-icons';
import { useState } from 'react';
import { Animated, Pressable, StyleSheet } from 'react-native';
import { colors } from '@/theme';

type Props = {
  onPress: () => void;
  size?: number;
  disabled?: boolean;
};

/** Botão "+" das listagens de produtos, com pulso ao adicionar. */
export function AddToCartButton({ onPress, size = 34, disabled = false }: Props) {
  const [escala] = useState(() => new Animated.Value(1));

  function aoTocar() {
    if (disabled) return;

    escala.stopAnimation();
    escala.setValue(0.8);

    Animated.spring(escala, {
      toValue: 1,
      speed: 20,
      bounciness: 14,
      useNativeDriver: true,
    }).start();

    onPress();
  }

  return (
    <Pressable
      onPress={aoTocar}
      disabled={disabled}
      hitSlop={8}
      style={[s.botao, { width: size, height: size }, disabled && s.desabilitado]}
      accessibilityLabel={disabled ? 'Produto sem estoque' : 'Adicionar ao carrinho'}
      accessibilityState={{ disabled }}>
      <Animated.View style={{ transform: [{ scale: escala }] }}>
        <Ionicons name={disabled ? 'ban-outline' : 'add'} size={20} color="#fff" />
      </Animated.View>
    </Pressable>
  );
}

const s = StyleSheet.create({
  botao: {
    alignSelf: 'flex-end',
    backgroundColor: colors.primary,
    borderRadius: 8,
    alignItems: 'center',
    justifyContent: 'center',
  },
  desabilitado: { backgroundColor: colors.muted, opacity: 0.65 },
});
