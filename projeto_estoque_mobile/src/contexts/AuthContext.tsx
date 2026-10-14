import React, { createContext, useContext, useEffect, useState } from 'react';
import * as SecureStore from 'expo-secure-store';

type AuthContextType = {
  token: string | null;
  loading: boolean;
  signIn: (email: string, senha: string) => Promise<void>;
  signOut: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    SecureStore.getItemAsync('token').then((t) => {
      setToken(t);
      setLoading(false);
    });
  }, []);

  async function signIn(email: string, senha: string) {
    // TEMPORÁRIO: aceita qualquer login até a API ficar pronta
    await SecureStore.setItemAsync('token', 'token-de-teste');
    setToken('token-de-teste');
  }

  async function signOut() {
    await SecureStore.deleteItemAsync('token');
    setToken(null);
  }

  return (
    <AuthContext.Provider value={{ token, loading, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);