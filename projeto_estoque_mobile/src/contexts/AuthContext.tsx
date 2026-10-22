import AsyncStorage from '@react-native-async-storage/async-storage';
import * as SecureStore from 'expo-secure-store';
import React, { createContext, useContext, useEffect, useState } from 'react';

export type Usuario = {
  nome: string;
  email: string;
  /** Vira `true` quando o nome é alterado em "Gerenciar conta". */
  nomePersonalizado?: boolean;
};

/** Nome inicial da conta até o usuário personalizar. */
export const NOME_PADRAO = 'Nelson';

type AuthContextType = {
  token: string | null;
  usuario: Usuario | null;
  loading: boolean;
  signIn: (email: string, senha: string, nome?: string) => Promise<void>;
  signOut: () => Promise<void>;
  /** Atualiza os dados do perfil (por enquanto só o nome). */
  atualizarPerfil: (dados: { nome: string }) => Promise<void>;
  /** Troca a senha mock, exigindo a senha atual. */
  alterarSenha: (atual: string, nova: string) => Promise<void>;
  /** Apaga token, perfil e senha locais. */
  excluirConta: () => Promise<void>;
};

const CHAVE_TOKEN = 'token';
const CHAVE_USUARIO = 'usuario';
const CHAVE_SENHA = 'senha_mock';

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let ativo = true;

    Promise.all([
      SecureStore.getItemAsync(CHAVE_TOKEN),
      AsyncStorage.getItem(CHAVE_USUARIO),
    ]).then(([tokenSalvo, usuarioSalvo]) => {
      if (!ativo) return;

      let perfil: Usuario | null = null;
      if (usuarioSalvo) {
        try {
          perfil = JSON.parse(usuarioSalvo);
        } catch {
          perfil = null;
        }
      }

      // Perfis antigos tinham o nome derivado do e-mail. Quem nunca
      // personalizou passa a começar com o nome padrão.
      if (perfil && !perfil.nomePersonalizado) {
        perfil = { ...perfil, nome: NOME_PADRAO };
        void AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(perfil));
      }

      setToken(tokenSalvo);
      setUsuario(perfil);
      setLoading(false);
    });

    return () => {
      ativo = false;
    };
  }, []);

  async function signIn(email: string, senha: string, nome?: string) {
    // TEMPORÁRIO: aceita qualquer login até a API ficar pronta.
    const emailLimpo = email.trim();
    const nomeInformado = nome?.trim();

    const perfil: Usuario =
      usuario && usuario.email === emailLimpo
        ? {
            ...usuario,
            // Perfis sem nome personalizado sempre partem de "Nelson".
            nome: usuario.nomePersonalizado ? usuario.nome : NOME_PADRAO,
          }
        : {
            nome: nomeInformado || NOME_PADRAO,
            email: emailLimpo,
            nomePersonalizado: Boolean(nomeInformado),
          };

    await Promise.all([
      SecureStore.setItemAsync(CHAVE_TOKEN, 'token-de-teste'),
      SecureStore.setItemAsync(CHAVE_SENHA, senha),
      AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(perfil)),
    ]);

    setToken('token-de-teste');
    setUsuario(perfil);
  }

  async function signOut() {
    // Mantém o perfil salvo para o próximo login; só remove a sessão.
    await SecureStore.deleteItemAsync(CHAVE_TOKEN);
    setToken(null);
  }

  async function atualizarPerfil(dados: { nome: string }) {
    if (!usuario) return;

    const atualizado: Usuario = {
      ...usuario,
      nome: dados.nome.trim(),
      nomePersonalizado: true,
    };
    await AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(atualizado));
    setUsuario(atualizado);
  }

  async function alterarSenha(atual: string, nova: string) {
    const senhaSalva = await SecureStore.getItemAsync(CHAVE_SENHA);

    if (senhaSalva && senhaSalva !== atual) {
      throw new Error('A senha atual está incorreta.');
    }

    await SecureStore.setItemAsync(CHAVE_SENHA, nova);
  }

  async function excluirConta() {
    await Promise.all([
      SecureStore.deleteItemAsync(CHAVE_TOKEN),
      SecureStore.deleteItemAsync(CHAVE_SENHA),
      AsyncStorage.removeItem(CHAVE_USUARIO),
    ]);

    setUsuario(null);
    setToken(null);
  }

  return (
    <AuthContext.Provider
      value={{
        token,
        usuario,
        loading,
        signIn,
        signOut,
        atualizarPerfil,
        alterarSenha,
        excluirConta,
      }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
