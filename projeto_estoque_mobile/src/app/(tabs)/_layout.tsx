import { Ionicons } from '@expo/vector-icons';
import { Tabs } from 'expo-router';
import { colors } from '@/theme';

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        // Troca de aba em crossfade suave.
        animation: 'fade',
        sceneStyle: { backgroundColor: colors.bg },
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.tabInactive,
        // Fundo azul claro (mesma paleta do painel web).
        tabBarStyle: {
          backgroundColor: colors.tabBg,
          borderTopColor: colors.tabBorder,
          borderTopWidth: 1,
          elevation: 8,
          shadowColor: '#1D4ED8',
          shadowOpacity: 0.12,
          shadowRadius: 8,
          shadowOffset: { width: 0, height: -2 },
        },
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
      }}>
      <Tabs.Screen
        name="index"
        options={{
          title: 'Início',
          tabBarIcon: ({ color, size }) => <Ionicons name="home" color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="categorias"
        options={{
          title: 'Categorias',
          tabBarIcon: ({ color, size }) => <Ionicons name="grid" color={color} size={size} />,
        }}
      />
      {/* O carrinho fica fora da barra inferior: o acesso é pelo ícone no topo
          (ScreenLayout) e pelo sidebar. `href: null` esconde a aba sem remover
          a rota, então `/(tabs)/carrinho` continua funcionando. */}
      <Tabs.Screen name="carrinho" options={{ href: null, title: 'Carrinho' }} />
      <Tabs.Screen
        name="perfil"
        options={{
          title: 'Perfil',
          tabBarIcon: ({ color, size }) => <Ionicons name="person" color={color} size={size} />,
        }}
      />
    </Tabs>
  );
}
