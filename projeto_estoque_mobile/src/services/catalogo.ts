import { api } from '@/services/api';
import type { Categoria, OpcaoCategoria, Produto } from '@/data/catalogo';

type ProdutoApi = {
  id: number;
  nome: string;
  categoriaId: number;
  categoriaNome: string;
  descricao: string | null;
  preco: number;
  precoPromocional: number | null;
  emPromocao: boolean;
  quantidade: number;
  status: string;
  statusClasse: string;
};

type PaginaProdutosApi = {
  itens: ProdutoApi[];
  total: number;
  pagina: number;
  totalPaginas: number;
  itensPorPagina: number;
};

type CategoriaApi = {
  id: number;
  nome: string;
  descricao: string;
  ativo: boolean;
  produtosVinculados: number;
};

type PaginaCategoriasApi = {
  itens: CategoriaApi[];
  total: number;
  pagina: number;
  totalPaginas: number;
  itensPorPagina: number;
};

function mapearProduto(produto: ProdutoApi): Produto {
  return {
    id: produto.id,
    nome: produto.nome,
    descricao: produto.descricao,
    preco: produto.preco,
    precoPromocional: produto.precoPromocional,
    emPromocao: produto.emPromocao,
    quantidade: produto.quantidade,
    categoriaId: produto.categoriaId,
    categoriaNome: produto.categoriaNome,
    status: produto.status,
    statusClasse: produto.statusClasse,
  };
}

function mapearCategoria(categoria: CategoriaApi): Categoria {
  return {
    id: categoria.id,
    nome: categoria.nome,
    descricao: categoria.descricao,
    produtosVinculados: categoria.produtosVinculados,
  };
}

export type FiltroProdutos = {
  busca?: string;
  categoriaId?: number;
  emPromocao?: boolean;
  pagina?: number;
  itensPorPagina?: number;
};

export type PaginaProdutos = {
  itens: Produto[];
  total: number;
  pagina: number;
  totalPaginas: number;
};

export async function listarProdutos(filtro: FiltroProdutos = {}): Promise<PaginaProdutos> {
  const { data } = await api.get<PaginaProdutosApi>('/produtos', {
    params: {
      busca: filtro.busca || undefined,
      categoriaId: filtro.categoriaId,
      emPromocao: filtro.emPromocao,
      pagina: filtro.pagina ?? 1,
      itensPorPagina: filtro.itensPorPagina ?? 8,
    },
  });

  return {
    itens: data.itens.map(mapearProduto),
    total: data.total,
    pagina: data.pagina,
    totalPaginas: data.totalPaginas,
  };
}

export async function listarCategorias(): Promise<Categoria[]> {
  const { data } = await api.get<PaginaCategoriasApi>('/categorias', {
    params: { pagina: 1, itensPorPagina: 100 },
  });

  return data.itens.map(mapearCategoria);
}

export async function obterCategoria(id: number): Promise<Categoria> {
  const { data } = await api.get<CategoriaApi>(`/categorias/${id}`);
  return mapearCategoria(data);
}

export async function listarOpcoesCategorias(): Promise<OpcaoCategoria[]> {
  const { data } = await api.get<OpcaoCategoria[]>('/categorias/opcoes');
  return data;
}
