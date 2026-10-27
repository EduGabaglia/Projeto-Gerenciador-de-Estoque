import { api } from '@/services/api';

export type Empresa = {
  id: number;
  preenchido: boolean;
  nome: string;
  nomeFantasia: string | null;
  cnpj: string;
  cep: string;
  endereco: string;
  numero: string | null;
  complemento: string | null;
  bairro: string | null;
  cidade: string;
  uf: string;
  telefone: string | null;
  email: string | null;
};

/** Dados públicos da loja usados nas telas de retirada. */
export async function obterEmpresa(): Promise<Empresa> {
  const { data } = await api.get<Empresa>('/configuracoes/empresa');
  return data;
}
