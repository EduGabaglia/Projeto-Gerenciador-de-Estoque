import { Ionicons } from '@expo/vector-icons';
import { usePathname, useRouter } from 'expo-router';
import type { ComponentProps } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { NOME_PADRAO, useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

type ItemMenu = {
  label: string;
  icon: ComponentProps<typeof Ionicons>['name'];
  href:
    | '/(tabs)'
    | '/(tabs)/categorias'
    | '/(tabs)/carrinho'
    | '/(tabs)/perfil'
    | '/compras';
  /** Caminhos (sem grupos) que deixam o item ativo. */
  match: string[];
};

const MENU: ItemMenu[] = [
  { label: 'Início', icon: 'home-outline', href: '/(tabs)', match: ['/'] },
  { label: 'Categorias', icon: 'grid-outline', href: '/(tabs)/categorias', match: ['/categorias'] },
  { label: 'Carrinho', icon: 'cart-outline', href: '/(tabs)/carrinho', match: ['/carrinho'] },
  { label: 'Minhas compras', icon: 'receipt-outline', href: '/compras', match: ['/compras', '/pedido'] },
  { label: 'Perfil', icon: 'person-outline', href: '/(tabs)/perfil', match: ['/perfil'] },
];

type Props = {
  /** Fecha o menu ao navegar (modo drawer). */
  onNavigate?: () => void;
  /** Exibe o botão de fechar (modo drawer). */
  onClose?: () => void;
};

export function AppSidebar({ onNavigate, onClose }: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const { signOut, usuario } = useAuth();

  function estaAtivo(match: string[]) {
    return match.some((m) => pathname === m || pathname.startsWith(`${m}/`));
  }

  function navegar(href: ItemMenu['href']) {
    onNavigate?.();
    router.push(href);
  }

  return (
    <View style={s.container}>
      {/* CABEÇALHO */}
      <View style={s.header}>
        <View style={s.logo}>
          <Text style={s.logoIcon}>📦</Text>
          <View style={s.logoTexto}>
            <Text style={s.logoTitulo} numberOfLines={1}>
              Super Atacadão
            </Text>
            <Text style={s.logoSub}>Loja online</Text>
          </View>
        </View>

        {onClose ? (
          <Pressable onPress={onClose} hitSlop={12} style={s.fechar} accessibilityLabel="Fechar menu">
            <Ionicons name="close" size={24} color={colors.sidebarText} />
          </Pressable>
        ) : null}
      </View>

      {/* MENU */}
      <View style={s.menu}>
        <Text style={s.secao}>MENU</Text>

        {MENU.map((item) => {
          const ativo = estaAtivo(item.match);

          return (
            <Pressable
              key={item.label}
              onPress={() => navegar(item.href)}
              style={({ pressed }) => [
                s.item,
                ativo && s.itemAtivo,
                pressed && !ativo && s.itemPressionado,
              ]}
              accessibilityRole="button"
              accessibilityState={{ selected: ativo }}>
              <Ionicons
                name={item.icon}
                size={20}
                color={ativo ? colors.sidebarText : colors.sidebarMuted}
              />
              <Text style={[s.itemTexto, ativo && s.itemTextoAtivo]}>{item.label}</Text>
            </Pressable>
          );
        })}
      </View>

      {/* RODAPÉ */}
      <View style={s.footer}>
        <Pressable
          onPress={() => navegar('/(tabs)/perfil')}
          style={({ pressed }) => [s.usuario, pressed && s.itemPressionado]}>
          <View style={s.avatar}>
            <Ionicons name="person" size={18} color={colors.sidebarText} />
          </View>
          <View style={s.usuarioInfo}>
            <Text style={s.usuarioNome} numberOfLines={1}>
              {usuario?.nome ?? NOME_PADRAO}
            </Text>
            <Text style={s.usuarioPapel}>Ver perfil</Text>
          </View>
        </Pressable>

        <Pressable
          onPress={() => {
            onNavigate?.();
            signOut();
          }}
          style={({ pressed }) => [s.sair, pressed && { opacity: 0.75 }]}>
          <Ionicons name="log-out-outline" size={18} color={colors.sidebarText} />
          <Text style={s.sairTexto}>Sair</Text>
        </Pressable>
      </View>
    </View>
  );
}

const s = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.sidebar,
  },

  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 18,
    borderBottomWidth: 1,
    borderBottomColor: '#262626',
  },
  logo: { flexDirection: 'row', alignItems: 'center', gap: 10, flexShrink: 1 },
  logoIcon: { fontSize: 24 },
  logoTexto: { flexShrink: 1, minWidth: 0 },
  logoTitulo: { color: colors.sidebarText, fontSize: 16, fontWeight: '700' },
  logoSub: { color: colors.sidebarMuted, fontSize: 11, marginTop: 1 },
  fechar: { padding: 2 },

  menu: { flex: 1, paddingHorizontal: 10, paddingTop: 18 },
  secao: {
    color: '#777777',
    fontSize: 10,
    fontWeight: '700',
    letterSpacing: 1,
    paddingHorizontal: 10,
    marginBottom: 8,
  },
  item: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 11,
    paddingVertical: 11,
    paddingHorizontal: 11,
    borderRadius: 8,
    marginBottom: 4,
  },
  itemAtivo: { backgroundColor: colors.sidebarActive },
  itemPressionado: { backgroundColor: '#1F1F1F' },
  itemTexto: { color: colors.sidebarMuted, fontSize: 14, fontWeight: '500' },
  itemTextoAtivo: { color: colors.sidebarText, fontWeight: '700' },

  footer: {
    paddingHorizontal: 12,
    paddingVertical: 14,
    borderTopWidth: 1,
    borderTopColor: '#262626',
  },
  usuario: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    paddingVertical: 6,
    borderRadius: 8,
  },
  avatar: {
    width: 34,
    height: 34,
    borderRadius: 17,
    backgroundColor: '#2563EB',
    alignItems: 'center',
    justifyContent: 'center',
  },
  usuarioInfo: { flexShrink: 1, minWidth: 0 },
  usuarioNome: { color: colors.sidebarText, fontSize: 13, fontWeight: '700' },
  usuarioPapel: { color: colors.sidebarMuted, fontSize: 11, marginTop: 1 },

  sair: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginTop: 12,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#333333',
  },
  sairTexto: { color: colors.sidebarText, fontSize: 13, fontWeight: '600' },
});
