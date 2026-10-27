import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useContext, useEffect, useState } from 'react';
import {
  CHAVE_USUARIO,
  chavePedidosCache,
  lerToken,
  limparSessaoLocal,
  salvarToken,
} from '@/services/armazenamento';
import {
  ApiError,
  definirTratadorNaoAutorizado,
  mensagemDeErro,
} from '@/services/api';
import { authService, type RespostaAuthApi } from '@/services/auth';

export type Usuario = {
  id: number;
  nome: string;
  email: string;
  telefone: string | null;
};

type AuthContextType = {
  token: string | null;
  usuario: Usuario | null;
  loading: boolean;
  signIn: (email: string, senha: string) => Promise<void>;
  signUp: (nome: string, email: string, senha: string) => Promise<void>;
  signOut: () => Promise<void>;
  atualizarPerfil: (dados: { nome: string; telefone?: string | null }) => Promise<void>;
  alterarSenha: (atual: string, nova: string) => Promise<void>;
  excluirConta: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

function lerUsuarioCache(bruto: string | null): Usuario | null {
  if (!bruto) return null;

  try {
    const valor = JSON.parse(bruto) as Partial<Usuario>;

    if (
      typeof valor.id === 'number' &&
      typeof valor.nome === 'string' &&
      typeof valor.email === 'string'
    ) {
      return {
        id: valor.id,
        nome: valor.nome,
        email: valor.email,
        telefone: typeof valor.telefone === 'string' ? valor.telefone : null,
      };
    }
  } catch {
    // Cache antigo/corrompido: ignora e busca o perfil na API.
  }

  return null;
}

async function gravarSessao(resposta: RespostaAuthApi) {
  await Promise.all([
    salvarToken(resposta.token),
    AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(resposta.usuario)),
  ]);
}

async function apagarSessao(usuarioId?: number) {
  await limparSessaoLocal();

  if (usuarioId) await AsyncStorage.removeItem(chavePedidosCache(usuarioId));
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    definirTratadorNaoAutorizado(() => {
      void limparSessaoLocal();
      setToken(null);
      setUsuario(null);
    });

    return () => definirTratadorNaoAutorizado(null);
  }, []);

  useEffect(() => {
    let ativo = true;

    async function iniciar() {
      const tokenSalvo = await lerToken();

      if (!tokenSalvo) {
        if (ativo) setLoading(false);
        return;
      }

      const cache = await AsyncStorage.getItem(CHAVE_USUARIO);
      const usuarioCache = lerUsuarioCache(cache);

      if (ativo && usuarioCache) {
        setToken(tokenSalvo);
        setUsuario(usuarioCache);
      }

      try {
        const perfil = await authService.me();

        if (!ativo) return;

        setToken(tokenSalvo);
        setUsuario(perfil);
        await AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(perfil));
      } catch (erro) {
        if (!ativo) return;

        if (erro instanceof ApiError && erro.status === 401) {
          await apagarSessao();
          setToken(null);
          setUsuario(null);
        } else if (!usuarioCache) {
          // Sem cache e sem servidor: mantém o token para o usuário tentar
          // novamente quando a conexão voltar, sem forçar um novo login.
          setToken(tokenSalvo);
        }
      } finally {
        if (ativo) setLoading(false);
      }
    }

    void iniciar();

    return () => {
      ativo = false;
    };
  }, []);

  async function signIn(email: string, senha: string) {
    const resposta = await authService.login(email.trim(), senha);

    await gravarSessao(resposta);
    setToken(resposta.token);
    setUsuario(resposta.usuario);
  }

  async function signUp(nome: string, email: string, senha: string) {
    const resposta = await authService.cadastrar(nome.trim(), email.trim(), senha);

    await gravarSessao(resposta);
    setToken(resposta.token);
    setUsuario(resposta.usuario);
  }

  async function signOut() {
    try {
      await apagarSessao(usuario?.id);
    } finally {
      setToken(null);
      setUsuario(null);
    }
  }

  async function atualizarPerfil(dados: { nome: string; telefone?: string | null }) {
    if (!usuario) throw new Error('Sessão expirada. Entre novamente.');

    const perfil = await authService.atualizarPerfil({
      nome: dados.nome.trim(),
      telefone: dados.telefone,
    });

    await AsyncStorage.setItem(CHAVE_USUARIO, JSON.stringify(perfil));
    setUsuario(perfil);
  }

  async function alterarSenha(atual: string, nova: string) {
    try {
      await authService.alterarSenha(atual, nova);
    } catch (erro) {
      throw new Error(mensagemDeErro(erro));
    }
  }

  async function excluirConta() {
    await authService.excluirConta();

    try {
      await apagarSessao(usuario?.id);
    } finally {
      setToken(null);
      setUsuario(null);
    }
  }

  return (
    <AuthContext.Provider
      value={{
        token,
        usuario,
        loading,
        signIn,
        signUp,
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
