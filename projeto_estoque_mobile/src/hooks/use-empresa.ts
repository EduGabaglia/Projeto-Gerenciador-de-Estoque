import { useEffect, useState } from 'react';
import { obterEmpresa, type Empresa } from '@/services/configuracoes';

/** Carrega os dados da loja sem bloquear a tela em caso de falha. */
export function useEmpresa() {
  const [empresa, setEmpresa] = useState<Empresa | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    let ativo = true;

    obterEmpresa()
      .then((dados) => {
        if (ativo) setEmpresa(dados);
      })
      .catch(() => {
        if (ativo) setEmpresa(null);
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, []);

  return { empresa, carregando };
}
