import { api } from '@/services/api';

export type UsuarioApi = {
  id: number;
  nome: string;
  email: string;
  telefone: string | null;
};

export type RespostaAuthApi = {
  token: string;
  usuario: UsuarioApi;
};

export const authService = {
  async login(email: string, senha: string) {
    const { data } = await api.post<RespostaAuthApi>('/auth/login', { email, senha });
    return data;
  },

  async cadastrar(nome: string, email: string, senha: string) {
    const { data } = await api.post<RespostaAuthApi>('/auth/cadastro', {
      nome,
      email,
      senha,
    });
    return data;
  },

  async me() {
    const { data } = await api.get<UsuarioApi>('/auth/me');
    return data;
  },

  async atualizarPerfil(dados: { nome: string; telefone?: string | null }) {
    const { data } = await api.put<UsuarioApi>('/auth/perfil', dados);
    return data;
  },

  async alterarSenha(senhaAtual: string, novaSenha: string) {
    await api.put('/auth/senha', { senhaAtual, novaSenha });
  },

  async excluirConta() {
    await api.delete('/auth/conta');
  },
};
