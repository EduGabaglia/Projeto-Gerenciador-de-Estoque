import { useWindowDimensions } from 'react-native';
import { SIDEBAR_BREAKPOINT, SIDEBAR_WIDTH } from '@/theme';

/**
 * Calcula a largura útil da área de conteúdo (descontando o sidebar fixo)
 * e quantas colunas a grade de produtos deve ter.
 */
export function useGrade() {
  const { width } = useWindowDimensions();

  const largura = width >= SIDEBAR_BREAKPOINT ? width - SIDEBAR_WIDTH : width;

  const colunas = largura < 460 ? 2 : largura < 720 ? 3 : largura < 980 ? 4 : 5;

  return { largura, colunas };
}
