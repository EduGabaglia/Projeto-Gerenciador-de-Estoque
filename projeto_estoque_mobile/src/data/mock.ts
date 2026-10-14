export type Produto = {
  id: string;
  nome: string;
  preco: number;
  categoria: 'Alimentos' | 'Bebidas' | 'Higiene' | 'Limpeza';
  destaque?: boolean;
};

export const categorias = ['Alimentos', 'Bebidas', 'Higiene', 'Limpeza'] as const;

export const produtos: Produto[] = [
  { id: '1', nome: 'Arroz 5kg', preco: 22.9, categoria: 'Alimentos', destaque: true },
  { id: '2', nome: 'Feijão 1kg', preco: 8.9, categoria: 'Alimentos', destaque: true },
  { id: '3', nome: 'Açúcar 1kg', preco: 4.5, categoria: 'Alimentos' },
  { id: '4', nome: 'Óleo de cozinha', preco: 7.5, categoria: 'Alimentos' },
  { id: '5', nome: 'Refrigerante 2L', preco: 9.9, categoria: 'Bebidas' },
  { id: '6', nome: 'Suco de uva 1L', preco: 7.2, categoria: 'Bebidas' },
  { id: '7', nome: 'Sabonete', preco: 2.5, categoria: 'Higiene' },
  { id: '8', nome: 'Creme dental', preco: 5.9, categoria: 'Higiene' },
  { id: '9', nome: 'Detergente', preco: 2.9, categoria: 'Limpeza' },
  { id: '10', nome: 'Água sanitária 1L', preco: 6.5, categoria: 'Limpeza' },
];