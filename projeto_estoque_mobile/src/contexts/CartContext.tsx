import React, { createContext, useContext, useState } from 'react';
import { Produto } from '@/data/mock';

type Item = { produto: Produto; qtd: number };

type CartType = {
  itens: Item[];
  add: (p: Produto) => void;
  alterarQtd: (id: string, delta: number) => void;
  limpar: () => void;
  subtotal: number;
  frete: number;
  total: number;
};

const CartContext = createContext<CartType>({} as CartType);

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [itens, setItens] = useState<Item[]>([]);

  const add = (p: Produto) =>
    setItens((prev) => {
      const existe = prev.find((i) => i.produto.id === p.id);
      if (existe) return prev.map((i) => (i.produto.id === p.id ? { ...i, qtd: i.qtd + 1 } : i));
      return [...prev, { produto: p, qtd: 1 }];
    });

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
    <CartContext.Provider value={{ itens, add, alterarQtd, limpar, subtotal, frete, total: subtotal + frete }}>
      {children}
    </CartContext.Provider>
  );
}

export const useCart = () => useContext(CartContext);