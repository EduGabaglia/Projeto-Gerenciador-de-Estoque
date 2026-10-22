import { Stack, useRouter, useSegments } from 'expo-router';
import { useEffect } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AuthProvider, useAuth } from '@/contexts/AuthContext';
import { CartProvider } from '@/contexts/CartContext';
import { PedidosProvider } from '@/contexts/PedidosContext';
import { colors } from '@/theme';
import { iniciarSincronizacaoAutomatica } from '@/sync/syncQueue';

function Guard() {
  const { token, loading } = useAuth();
  const segments = useSegments();
  const router = useRouter();

  useEffect(() => {
    const parar = iniciarSincronizacaoAutomatica();
    return () => parar();
  }, []);

  useEffect(() => {
    if (loading) return;
    const naAreaAuth = segments[0] === '(auth)';
    if (!token && !naAreaAuth) router.replace('/(auth)/login');
    if (token && naAreaAuth) router.replace('/(tabs)');
  }, [token, loading, segments, router]);

  // Um Stack de verdade mantém a tela anterior montada. Sem isso, abrir uma
  // rota fora de `(tabs)` desmonta o navegador de abas e o voltar cai sempre
  // na primeira aba (Início), perdendo a aba de origem.
  return (
    <Stack
      screenOptions={{
        headerShown: false,
        // Slide lateral consistente entre páginas; login/cadastro usam fade.
        animation: 'slide_from_right',
        animationDuration: 250,
        gestureEnabled: true,
        contentStyle: { backgroundColor: colors.bg },
      }}>
      {/* Login/cadastro entram com fade; o resto desliza da direita. */}
      <Stack.Screen name="(auth)/login" options={{ animation: 'fade' }} />
      <Stack.Screen name="(auth)/cadastro" options={{ animation: 'fade' }} />
    </Stack>
  );
}

export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <AuthProvider>
        <PedidosProvider>
          <CartProvider>
            <Guard />
          </CartProvider>
        </PedidosProvider>
      </AuthProvider>
    </SafeAreaProvider>
  );
}
