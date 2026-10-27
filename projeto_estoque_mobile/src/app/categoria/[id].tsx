import { Ionicons } from '@expo/vector-icons';
import { useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { AddToCartButton } from '@/components/add-to-cart-button';
import { PrecoProduto } from '@/components/preco-produto';
import { ScreenLayout } from '@/components/screen-layout';
import { useCart } from '@/contexts/CartContext';
import type { Categoria, Produto } from '@/data/catalogo';
import { mensagemDeErro } from '@/services/api';
import { listarProdutos, obterCategoria } from '@/services/catalogo';
import { colors } from '@/theme';

const ITENS_POR_PAGINA = 20;

export default function CategoriaDetalhe() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const categoriaId = Number(id);
  const idValido = Number.isInteger(categoriaId) && categoriaId > 0;

  const { add, quantidadeDe } = useCart();

  const [categoria, setCategoria] = useState<Categoria | null>(null);
  const [produtos, setProdutos] = useState<Produto[]>([]);
  const [total, setTotal] = useState(0);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(1);
  const [carregando, setCarregando] = useState(idValido);
  const [carregandoMais, setCarregandoMais] = useState(false);
  const [atualizando, setAtualizando] = useState(false);
  const [erro, setErro] = useState<string | null>(
    idValido ? null : 'Categoria inválida.'
  );

  async function carregar() {
    if (!idValido) return;

    setCarregando(true);
    setErro(null);

    try {
      const [detalhe, resultado] = await Promise.all([
        obterCategoria(categoriaId),
        listarProdutos({
          categoriaId,
          pagina: 1,
          itensPorPagina: ITENS_POR_PAGINA,
        }),
      ]);

      setCategoria(detalhe);
      setProdutos(resultado.itens);
      setTotal(resultado.total);
      setPagina(resultado.pagina);
      setTotalPaginas(resultado.totalPaginas);
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    if (!idValido) return;

    let ativo = true;

    Promise.all([
      obterCategoria(categoriaId),
      listarProdutos({
        categoriaId,
        pagina: 1,
        itensPorPagina: ITENS_POR_PAGINA,
      }),
    ])
      .then(([detalhe, resultado]) => {
        if (!ativo) return;

        setCategoria(detalhe);
        setProdutos(resultado.itens);
        setTotal(resultado.total);
        setPagina(resultado.pagina);
        setTotalPaginas(resultado.totalPaginas);
        setErro(null);
      })
      .catch((falha) => {
        if (!ativo) return;
        setErro(mensagemDeErro(falha));
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [categoriaId, idValido]);

  async function atualizar() {
    setAtualizando(true);
    await carregar();
    setAtualizando(false);
  }

  async function carregarMais() {
    if (carregando || carregandoMais || pagina >= totalPaginas) return;

    setCarregandoMais(true);

    try {
      const resultado = await listarProdutos({
        categoriaId,
        pagina: pagina + 1,
        itensPorPagina: ITENS_POR_PAGINA,
      });

      setProdutos((atuais) => {
        const ids = new Set(atuais.map((produto) => produto.id));
        return [...atuais, ...resultado.itens.filter((produto) => !ids.has(produto.id))];
      });
      setPagina(resultado.pagina);
      setTotalPaginas(resultado.totalPaginas);
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setCarregandoMais(false);
    }
  }

  return (
    <ScreenLayout
      title={categoria?.nome ?? 'Categoria'}
      subtitle={`${total} ${total === 1 ? 'produto' : 'produtos'}`}
      mostrarVoltar
      voltarPara="/(tabs)/categorias">
      <FlatList
        style={s.container}
        data={produtos}
        keyExtractor={(produto) => String(produto.id)}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={atualizando} onRefresh={atualizar} tintColor={colors.primary} />
        }
        ListHeaderComponent={
          <>
            {categoria?.descricao ? <Text style={s.descricao}>{categoria.descricao}</Text> : null}
            {erro ? (
              <View style={s.erroBox}>
                <Ionicons name="cloud-offline-outline" size={20} color={colors.danger} />
                <View style={s.erroTextoBox}>
                  <Text style={s.erroTitulo}>Não foi possível carregar a categoria</Text>
                  <Text style={s.erroTexto}>{erro}</Text>
                </View>
                <TouchableOpacity onPress={() => void carregar()}>
                  <Text style={s.erroAcao}>Tentar</Text>
                </TouchableOpacity>
              </View>
            ) : null}
          </>
        }
        ListEmptyComponent={
          carregando ? (
            <View style={s.vazio}>
              <ActivityIndicator color={colors.primary} size="large" />
            </View>
          ) : erro ? null : (
            <View style={s.vazio}>
              <Ionicons name="cube-outline" size={44} color={colors.muted} />
              <Text style={s.vazioTitulo}>Nenhum produto nesta categoria</Text>
            </View>
          )
        }
        ListFooterComponent={
          carregandoMais ? (
            <ActivityIndicator style={s.rodape} color={colors.primary} />
          ) : produtos.length > 0 && pagina < totalPaginas ? (
            <TouchableOpacity
              style={s.carregarMais}
              activeOpacity={0.8}
              onPress={() => void carregarMais()}>
              <Text style={s.carregarMaisTxt}>Carregar mais produtos</Text>
            </TouchableOpacity>
          ) : (
            <View style={{ height: 24 }} />
          )
        }
        renderItem={({ item }) => {
          const semEstoque = item.quantidade <= 0;
          const limite = quantidadeDe(item.id) >= item.quantidade;

          return (
            <View style={s.linha}>
              <View style={{ flex: 1, minWidth: 0 }}>
                <View style={s.topo}>
                  <Text style={s.nome} numberOfLines={2}>
                    {item.nome}
                  </Text>
                  {item.emPromocao ? (
                    <View style={s.promoTag}>
                      <Text style={s.promoTagTxt}>PROMO</Text>
                    </View>
                  ) : null}
                </View>

                <PrecoProduto produto={item} tamanho={14} />

                <View style={s.estoqueLinha}>
                  <Text
                    style={[
                      s.estoque,
                      semEstoque
                        ? { color: colors.danger }
                        : item.quantidade <= 5
                          ? { color: colors.warning }
                          : { color: colors.success },
                    ]}>
                    {item.status}
                  </Text>
                  <Text style={s.estoqueQtd}>{item.quantidade} un.</Text>
                </View>
              </View>

              <AddToCartButton
                onPress={() => add(item)}
                disabled={semEstoque || limite}
                size={36}
              />
            </View>
          );
        }}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  descricao: { color: colors.muted, marginBottom: 12, lineHeight: 19 },
  linha: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 12,
    marginBottom: 10,
    gap: 12,
  },
  topo: { flexDirection: 'row', alignItems: 'flex-start', gap: 6, marginBottom: 3 },
  nome: { fontWeight: '600', color: colors.text, flex: 1 },
  promoTag: {
    backgroundColor: colors.primary,
    borderRadius: 999,
    paddingHorizontal: 6,
    paddingVertical: 2,
  },
  promoTagTxt: { color: '#fff', fontSize: 8, fontWeight: '800' },
  estoqueLinha: { flexDirection: 'row', gap: 8, marginTop: 4, alignItems: 'center' },
  estoque: { fontSize: 10, fontWeight: '700' },
  estoqueQtd: { fontSize: 10, color: colors.muted },
  erroBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    backgroundColor: colors.dangerBg,
    borderRadius: 10,
    padding: 12,
    marginBottom: 12,
  },
  erroTextoBox: { flex: 1 },
  erroTitulo: { color: colors.danger, fontWeight: '700', fontSize: 13 },
  erroTexto: { color: colors.danger, fontSize: 12, marginTop: 2 },
  erroAcao: { color: colors.danger, fontWeight: '700', fontSize: 12 },
  vazio: { alignItems: 'center', paddingVertical: 48, gap: 8 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text },
  rodape: { paddingVertical: 16 },
  carregarMais: {
    marginTop: 8,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: 10,
    padding: 12,
    alignItems: 'center',
  },
  carregarMaisTxt: { color: colors.primary, fontWeight: '700' },
});
