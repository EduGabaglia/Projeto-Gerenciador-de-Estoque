import { Ionicons } from '@expo/vector-icons';
import { useState } from 'react';
import { Animated, Pressable, StyleSheet } from 'react-native';
import { colors } from '@/theme';

type Props = {
  onPress: () => void;
  size?: number;
};

/** Botão "+" das listagens de produtos, com pulso ao adicionar. */
export function AddToCartButton({ onPress, size = 34 }: Props) {
  const [escala] = useState(() => new Animated.Value(1));

  function aoTocar() {
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
      hitSlop={8}
      style={[s.botao, { width: size, height: size }]}
      accessibilityLabel="Adicionar ao carrinho">
      <Animated.View style={{ transform: [{ scale: escala }] }}>
        <Ionicons name="add" size={20} color="#fff" />
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
});
