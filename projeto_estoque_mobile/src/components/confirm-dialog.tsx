import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Animated,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { colors } from '@/theme';

type Props = {
  visivel: boolean;
  titulo: string;
  mensagem: string;
  confirmar?: string;
  cancelar?: string;
  /** Usa vermelho no botão de confirmação. */
  destrutivo?: boolean;
  carregando?: boolean;
  onConfirmar: () => void;
  onCancelar: () => void;
};

/**
 * Diálogo de confirmação próprio (o `Alert` nativo não tem dois botões na web).
 * Entra com fade + leve zoom.
 */
export function ConfirmDialog({
  visivel,
  titulo,
  mensagem,
  confirmar = 'Confirmar',
  cancelar = 'Cancelar',
  destrutivo = false,
  carregando = false,
  onConfirmar,
  onCancelar,
}: Props) {
  const [opacidade] = useState(() => new Animated.Value(0));
  const [escala] = useState(() => new Animated.Value(0.94));

  useEffect(() => {
    if (!visivel) return;

    opacidade.setValue(0);
    escala.setValue(0.94);

    Animated.parallel([
      Animated.timing(opacidade, {
        toValue: 1,
        duration: 160,
        useNativeDriver: true,
      }),
      Animated.spring(escala, {
        toValue: 1,
        speed: 20,
        bounciness: 6,
        useNativeDriver: true,
      }),
    ]).start();
  }, [visivel, opacidade, escala]);

  return (
    <Modal
      visible={visivel}
      transparent
      animationType="fade"
      statusBarTranslucent
      onRequestClose={onCancelar}>
      <View style={s.fundo}>
        <Pressable
          style={StyleSheet.absoluteFill}
          onPress={carregando ? undefined : onCancelar}
          accessibilityLabel="Fechar diálogo"
        />

        <Animated.View
          accessibilityViewIsModal
          style={[s.card, { opacity: opacidade, transform: [{ scale: escala }] }]}>
          <Text style={s.titulo}>{titulo}</Text>
          <Text style={s.mensagem}>{mensagem}</Text>

          <View style={s.acoes}>
            <Pressable
              onPress={onCancelar}
              disabled={carregando}
              style={({ pressed }) => [s.botao, s.secundario, pressed && s.pressionado]}>
              <Text style={s.secundarioTxt}>{cancelar}</Text>
            </Pressable>

            <Pressable
              onPress={onConfirmar}
              disabled={carregando}
              style={({ pressed }) => [
                s.botao,
                destrutivo ? s.destrutivo : s.primario,
                (pressed || carregando) && s.pressionado,
              ]}>
              {carregando ? (
                <ActivityIndicator color="#fff" size="small" />
              ) : (
                <Text style={s.primarioTxt}>{confirmar}</Text>
              )}
            </Pressable>
          </View>
        </Animated.View>
      </View>
    </Modal>
  );
}

const s = StyleSheet.create({
  fundo: {
    flex: 1,
    backgroundColor: colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  card: {
    width: '100%',
    maxWidth: 400,
    backgroundColor: colors.bg,
    borderRadius: 16,
    padding: 20,
    shadowColor: '#0F172A',
    shadowOpacity: 0.2,
    shadowRadius: 24,
    shadowOffset: { width: 0, height: 10 },
    elevation: 8,
  },
  titulo: { fontSize: 18, fontWeight: '700', color: colors.text },
  mensagem: { fontSize: 14, color: colors.muted, marginTop: 8, lineHeight: 20 },
  acoes: { flexDirection: 'row', gap: 10, marginTop: 20 },
  botao: {
    flex: 1,
    minHeight: 44,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 12,
  },
  secundario: { borderWidth: 1, borderColor: colors.border, backgroundColor: colors.bg },
  secundarioTxt: { color: colors.text, fontWeight: '600', fontSize: 14 },
  primario: { backgroundColor: colors.primary },
  primarioTxt: { color: '#fff', fontWeight: '700', fontSize: 14 },
  destrutivo: { backgroundColor: colors.danger },
  pressionado: { opacity: 0.8 },
});
