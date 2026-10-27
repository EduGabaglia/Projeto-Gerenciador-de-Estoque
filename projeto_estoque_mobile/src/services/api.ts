import { AxiosError, create } from 'axios';
import Constants from 'expo-constants';
import { Platform } from 'react-native';
import { lerToken } from '@/services/armazenamento';

/**
 * Endereço da API sem o sufixo /api.
 *
 * Ordem de prioridade:
 * 1. EXPO_PUBLIC_API_URL (obrigatório em builds de produção);
 * 2. IP do computador que está servindo o Expo (funciona no Expo Go);
 * 3. 10.0.2.2 no emulador Android (localhost do computador);
 * 4. localhost no iOS/web.
 */
function urlPadrao() {
  const hostUri = Constants.expoConfig?.hostUri;
  const host = hostUri?.split(':')[0];

  if (host) return `http://${host}:5080`;
  if (Platform.OS === 'android') return 'http://10.0.2.2:5080';

  return 'http://localhost:5080';
}

const configurada = process.env.EXPO_PUBLIC_API_URL?.trim();

export const API_URL = (configurada || urlPadrao())
  .replace(/\/+$/, '')
  .replace(/\/api$/i, '');

export class ApiError extends Error {
  constructor(
    mensagem: string,
    readonly status?: number
  ) {
    super(mensagem);
    this.name = 'ApiError';
  }
}

type CorpoErro = {
  mensagem?: string;
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
};

function extrairMensagem(corpo: unknown): string | null {
  if (typeof corpo === 'string') {
    const texto = corpo.trim();
    return texto && !texto.startsWith('<') ? texto : null;
  }

  if (!corpo || typeof corpo !== 'object') return null;

  const dados = corpo as CorpoErro;

  if (typeof dados.mensagem === 'string' && dados.mensagem.trim())
    return dados.mensagem.trim();

  if (dados.errors) {
    const mensagens = Object.values(dados.errors).flat().filter(Boolean);
    if (mensagens.length > 0) return mensagens.join(' ');
  }

  if (typeof dados.message === 'string' && dados.message.trim()) return dados.message.trim();
  if (typeof dados.title === 'string' && dados.title.trim()) return dados.title.trim();

  return null;
}

export function mensagemDeErro(erro: unknown) {
  if (erro instanceof Error && erro.message) return erro.message;

  return 'Não foi possível concluir a operação. Tente novamente.';
}

let tratarNaoAutorizado: (() => void) | null = null;

/** Permite que o AuthContext encerre a sessão quando a API responder 401. */
export function definirTratadorNaoAutorizado(tratador: (() => void) | null) {
  tratarNaoAutorizado = tratador;
}

export const api = create({
  baseURL: `${API_URL}/api`,
  timeout: 15000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(async (config) => {
  const token = await lerToken();

  if (token) config.headers.Authorization = `Bearer ${token}`;

  return config;
});

api.interceptors.response.use(
  (resposta) => resposta,
  (erro: AxiosError) => {
    const status = erro.response?.status;
    const url = erro.config?.url ?? '';
    const corpo = erro.response?.data;

    let mensagem = extrairMensagem(corpo);

    if (!mensagem) {
      if (erro.code === 'ECONNABORTED')
        mensagem = 'A conexão demorou demais. Verifique sua internet e tente novamente.';
      else if (!erro.response)
        mensagem = 'Não foi possível conectar ao servidor. Verifique sua internet.';
      else
        mensagem = 'Não foi possível concluir a operação. Tente novamente.';
    }

    const rotaDeLogin = url.includes('/auth/login') || url.includes('/auth/cadastro');

    if (status === 401 && !rotaDeLogin) tratarNaoAutorizado?.();

    return Promise.reject(new ApiError(mensagem, status));
  }
);
