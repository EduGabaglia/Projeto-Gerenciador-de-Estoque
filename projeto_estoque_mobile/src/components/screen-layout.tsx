import { Ionicons } from '@expo/vector-icons';
import { useRouter, type Href } from 'expo-router';
import type { ReactNode } from 'react';
import { useEffect, useState } from 'react';
import {
  Animated,
  Easing,
  Pressable,
  StyleSheet,
  Text,
  View,
  useWindowDimensions,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { AnimatedCartIcon } from '@/components/animated-cart-icon';
import { AppSidebar } from '@/components/app-sidebar';
import { colors, SIDEBAR_BREAKPOINT, SIDEBAR_WIDTH } from '@/theme';

type Props = {
  /** Título exibido na barra superior. */
  title: string;
  subtitle?: string;
  /** Ação do botão voltar (padrão: voltar ou ir para o início). */
  onBack?: () => void;
  /** Destino do voltar quando não há histórico (ex.: acesso por link direto). */
  voltarPara?: Href;
  /** Exibe o botão voltar na barra superior. */
  mostrarVoltar?: boolean;
  /** Exibe o atalho do carrinho na barra superior. */
  mostrarCarrinho?: boolean;
  children: ReactNode;
};

/**
 * Layout padrão das telas do app.
 *
 * - Em telas largas (>= 900px) o sidebar escuro fica fixo à esquerda.
 * - Em telas estreitas o sidebar vira um menu deslizante (drawer),
 *   aberto pelo botão de menu da barra superior.
 */
export function ScreenLayout({
  title,
  subtitle,
  onBack,
  voltarPara,
  mostrarVoltar = false,
  mostrarCarrinho = true,
  children,
}: Props) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();

  const telaLarga = width >= SIDEBAR_BREAKPOINT;
  const [menuAberto, setMenuAberto] = useState(false);
  const [progresso] = useState(() => new Animated.Value(0));

  // Em telas largas o menu é fixo, então o estado é sempre fechado.
  const [telaLargaAnterior, setTelaLargaAnterior] = useState(telaLarga);
  if (telaLarga !== telaLargaAnterior) {
    setTelaLargaAnterior(telaLarga);
    if (telaLarga) setMenuAberto(false);
  }

  const menuVisivel = !telaLarga && menuAberto;

  // Anima a entrada/saída do menu deslizante.
  useEffect(() => {
    Animated.timing(progresso, {
      toValue: menuVisivel ? 1 : 0,
      duration: 220,
      easing: Easing.out(Easing.cubic),
      useNativeDriver: true,
    }).start();
  }, [menuVisivel, progresso]);

  function voltar() {
    if (onBack) {
      onBack();
      return;
    }

    if (router.canGoBack()) {
      router.back();
    } else {
      router.replace(voltarPara ?? '/(tabs)');
    }
  }

  const cabecalho = (
    <View style={[s.cabecalho, { paddingTop: insets.top + 8 }]}>
      {!telaLarga ? (
        <Pressable
          onPress={() => setMenuAberto(true)}
          hitSlop={10}
          style={s.botaoMenu}
          accessibilityLabel="Abrir menu">
          <Ionicons name="menu" size={24} color={colors.text} />
        </Pressable>
      ) : null}

      {mostrarVoltar ? (
        <Pressable onPress={voltar} hitSlop={10} style={s.botaoMenu} accessibilityLabel="Voltar">
          <Ionicons name="arrow-back" size={22} color={colors.text} />
        </Pressable>
      ) : null}

      <View style={s.titulos}>
        <Text style={s.titulo} numberOfLines={1}>
          {title}
        </Text>
        {subtitle ? (
          <Text style={s.subtitulo} numberOfLines={1}>
            {subtitle}
          </Text>
        ) : null}
      </View>

      {mostrarCarrinho ? (
        <Pressable
          onPress={() => router.push('/(tabs)/carrinho')}
          hitSlop={10}
          style={s.botaoCarrinho}
          accessibilityLabel="Abrir carrinho">
          <AnimatedCartIcon size={24} />
        </Pressable>
      ) : null}
    </View>
  );

  const conteudo = <View style={s.conteudo}>{children}</View>;

  if (telaLarga) {
    return (
      <View style={s.linha}>
        <View style={[s.sidebarFixo, { paddingTop: insets.top }]}>
          <AppSidebar />
        </View>

        <View style={s.coluna}>
          {cabecalho}
          {conteudo}
        </View>
      </View>
    );
  }

  const deslocamento = progresso.interpolate({
    inputRange: [0, 1],
    outputRange: [-SIDEBAR_WIDTH, 0],
  });

  const opacidadeFundo = progresso.interpolate({
    inputRange: [0, 1],
    outputRange: [0, 1],
  });

  return (
    <View style={s.container}>
      {cabecalho}
      {conteudo}

      {/* MENU DESLIZANTE */}
      <Animated.View
        pointerEvents={menuVisivel ? 'auto' : 'none'}
        style={[
          s.drawer,
          { paddingTop: insets.top, paddingBottom: insets.bottom, transform: [{ translateX: deslocamento }] },
        ]}>
        <AppSidebar onNavigate={() => setMenuAberto(false)} onClose={() => setMenuAberto(false)} />
      </Animated.View>

      {/* FUNDO ESCURO */}
      <Animated.View
        pointerEvents={menuVisivel ? 'auto' : 'none'}
        style={[s.fundo, { opacity: opacidadeFundo }]}
      >
        <Pressable
          style={StyleSheet.absoluteFill}
          onPress={() => setMenuAberto(false)}
          accessibilityLabel="Fechar menu"
        />
      </Animated.View>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  linha: { flex: 1, flexDirection: 'row', backgroundColor: colors.bg },
  coluna: { flex: 1 },
  sidebarFixo: { width: SIDEBAR_WIDTH, backgroundColor: colors.sidebar },

  cabecalho: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingHorizontal: 16,
    paddingBottom: 12,
    backgroundColor: colors.bg,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  botaoMenu: { padding: 4 },
  botaoCarrinho: { padding: 2, marginLeft: 'auto' },
  titulos: { flex: 1, minWidth: 0 },
  titulo: { fontSize: 18, fontWeight: '700', color: colors.text },
  subtitulo: { fontSize: 12, color: colors.muted, marginTop: 1 },

  conteudo: { flex: 1 },

  drawer: {
    position: 'absolute',
    top: 0,
    bottom: 0,
    left: 0,
    width: SIDEBAR_WIDTH,
    zIndex: 30,
    elevation: 30,
    shadowColor: '#000',
    shadowOpacity: 0.25,
    shadowRadius: 12,
    shadowOffset: { width: 4, height: 0 },
  },
  fundo: {
    ...StyleSheet.absoluteFill,
    backgroundColor: colors.overlay,
    zIndex: 20,
    elevation: 20,
  },
});
