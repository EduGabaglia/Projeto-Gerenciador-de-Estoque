import { Slot, useRouter, useSegments } from 'expo-router';
import { useEffect } from 'react';
import { AuthProvider, useAuth } from '@/contexts/AuthContext';
import { CartProvider } from '@/contexts/CartContext';
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
  }, [token, loading, segments]);

  return <Slot />;
}

export default function RootLayout() {
  return (
    <AuthProvider>
      <CartProvider>
        <Guard />
      </CartProvider>
    </AuthProvider>
  );
}