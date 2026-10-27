import AsyncStorage from '@react-native-async-storage/async-storage';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

const CHAVE_TOKEN = 'token';

/** Chave do perfil em cache. A versão evita conflito com o formato antigo/mock. */
export const CHAVE_USUARIO = 'usuario_v2';

export const chavePedidosCache = (usuarioId: number) => `pedidos_v2_${usuarioId}`;

function armazenamentoWeb() {
  return typeof localStorage === 'undefined' ? null : localStorage;
}

/**
 * SecureStore é o armazenamento recomendado no Android/iOS. No web ele não é
 * suportado, então usamos o localStorage apenas para manter o app funcional.
 */
export async function lerToken() {
  try {
    if (Platform.OS === 'web') return armazenamentoWeb()?.getItem(CHAVE_TOKEN) ?? null;
    return await SecureStore.getItemAsync(CHAVE_TOKEN);
  } catch {
    return null;
  }
}

export async function salvarToken(token: string) {
  if (Platform.OS === 'web') {
    armazenamentoWeb()?.setItem(CHAVE_TOKEN, token);
    return;
  }

  await SecureStore.setItemAsync(CHAVE_TOKEN, token);
}

export async function removerToken() {
  if (Platform.OS === 'web') {
    armazenamentoWeb()?.removeItem(CHAVE_TOKEN);
    return;
  }

  await SecureStore.deleteItemAsync(CHAVE_TOKEN);
}

export async function limparSessaoLocal() {
  await Promise.all([removerToken(), AsyncStorage.removeItem(CHAVE_USUARIO)]);
}
