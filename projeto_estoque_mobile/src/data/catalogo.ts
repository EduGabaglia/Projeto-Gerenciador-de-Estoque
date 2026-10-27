export type Produto = {
  id: number;
  nome: string;
  descricao: string | null;
  preco: number;
  precoPromocional: number | null;
  emPromocao: boolean;
  quantidade: number;
  categoriaId: number;
  categoriaNome: string;
  status: string;
  statusClasse: string;
};

export type Categoria = {
  id: number;
  nome: string;
  descricao: string;
  produtosVinculados: number;
};

export type OpcaoCategoria = {
  id: number;
  nome: string;
};

/** Preço efetivamente cobrado (promocional quando houver). */
export const precoVenda = (produto: Produto) =>
  produto.precoPromocional ?? produto.preco;

export const produtoDisponivel = (produto: Produto) => produto.quantidade > 0;
