import React, { createContext, useContext, useState } from 'react';
import { Produto } from '@/data/mock';

export type Item = { produto: Produto; qtd: number };

type CartType = {
  itens: Item[];
  add: (p: Produto) => void;
  alterarQtd: (id: string, delta: number) => void;
  limpar: () => void;
  subtotal: number;
  frete: number;
  total: number;
  /** Quantidade total de unidades no carrinho (badge do ícone). */
  totalItens: number;
  /** Incrementa a cada item adicionado — usado para disparar a animação do carrinho. */
  adicoes: number;
};

const CartContext = createContext<CartType>({} as CartType);

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [itens, setItens] = useState<Item[]>([]);
  const [adicoes, setAdicoes] = useState(0);

  const add = (p: Produto) => {
    // Sinaliza a animação do ícone do carrinho antes de mexer na lista.
    setAdicoes((n) => n + 1);
    setItens((prev) => {
      const existe = prev.find((i) => i.produto.id === p.id);
      if (existe) return prev.map((i) => (i.produto.id === p.id ? { ...i, qtd: i.qtd + 1 } : i));
      return [...prev, { produto: p, qtd: 1 }];
    });
  };

  const alterarQtd = (id: string, delta: number) =>
    setItens((prev) =>
      prev
        .map((i) => (i.produto.id === id ? { ...i, qtd: i.qtd + delta } : i))
        .filter((i) => i.qtd > 0)
    );

  const limpar = () => setItens([]);
  const subtotal = itens.reduce((s, i) => s + i.produto.preco * i.qtd, 0);
  const frete = 0;

  return (
    <CartContext.Provider
      value={{
        itens,
        add,
        alterarQtd,
        limpar,
        subtotal,
        frete,
        total: subtotal + frete,
        totalItens: itens.reduce((s, i) => s + i.qtd, 0),
        adicoes,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export const useCart = () => useContext(CartContext);
