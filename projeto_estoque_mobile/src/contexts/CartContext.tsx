import React, { createContext, useContext, useState } from 'react';
import { precoVenda, type Produto } from '@/data/catalogo';

export type Item = { produto: Produto; qtd: number };

type CartType = {
  itens: Item[];
  add: (p: Produto) => void;
  alterarQtd: (id: number, delta: number) => void;
  limpar: () => void;
  subtotal: number;
  total: number;
  /** Quantidade total de unidades no carrinho (badge do ícone). */
  totalItens: number;
  /** Unidades de um produto já adicionadas. */
  quantidadeDe: (id: number) => number;
  /** Incrementa a cada item adicionado — usado para disparar a animação do carrinho. */
  adicoes: number;
};

const CartContext = createContext<CartType>({} as CartType);

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [itens, setItens] = useState<Item[]>([]);
  const [adicoes, setAdicoes] = useState(0);

  const add = (produto: Produto) => {
    const jaNoCarrinho = itens.find((i) => i.produto.id === produto.id)?.qtd ?? 0;

    if (produto.quantidade <= 0 || jaNoCarrinho >= produto.quantidade) return;

    // Sinaliza a animação do ícone do carrinho antes de mexer na lista.
    setAdicoes((n) => n + 1);
    setItens((prev) => {
      const existe = prev.find((i) => i.produto.id === produto.id);

      if (existe) {
        return prev.map((i) =>
          i.produto.id === produto.id
            ? { ...i, produto, qtd: Math.min(i.qtd + 1, produto.quantidade) }
            : i
        );
      }

      return [...prev, { produto, qtd: 1 }];
    });
  };

  const alterarQtd = (id: number, delta: number) => {
    setItens((prev) =>
      prev
        .map((i) => {
          if (i.produto.id !== id) return i;

          const limite = i.produto.quantidade;
          return { ...i, qtd: Math.min(i.qtd + delta, limite) };
        })
        .filter((i) => i.qtd > 0)
    );
  };

  const limpar = () => setItens([]);
  const quantidadeDe = (id: number) => itens.find((i) => i.produto.id === id)?.qtd ?? 0;
  const subtotal = itens.reduce((soma, i) => soma + precoVenda(i.produto) * i.qtd, 0);

  return (
    <CartContext.Provider
      value={{
        itens,
        add,
        alterarQtd,
        limpar,
        subtotal,
        total: subtotal,
        totalItens: itens.reduce((soma, i) => soma + i.qtd, 0),
        quantidadeDe,
        adicoes,
      }}>
      {children}
    </CartContext.Provider>
  );
}

export const useCart = () => useContext(CartContext);
