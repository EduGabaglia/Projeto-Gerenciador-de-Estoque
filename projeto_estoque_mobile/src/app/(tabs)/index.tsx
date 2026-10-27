import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { useEffect, useRef, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { AddToCartButton } from '@/components/add-to-cart-button';
import { PrecoProduto } from '@/components/preco-produto';
import { ScreenLayout } from '@/components/screen-layout';
import { useCart } from '@/contexts/CartContext';
import type { OpcaoCategoria, Produto } from '@/data/catalogo';
import { useGrade } from '@/hooks/use-grade';
import { mensagemDeErro } from '@/services/api';
import {
  listarOpcoesCategorias,
  listarProdutos,
  type FiltroProdutos,
} from '@/services/catalogo';
import { colors } from '@/theme';

const ITENS_POR_PAGINA = 8;
const ITENS_DESTAQUE = 100;

type ResultadoPrimeiraPagina = {
  itens: Produto[];
  total: number;
  pagina: number;
  totalPaginas: number;
};

async function buscarPrimeiraPagina(termoAtual: string): Promise<ResultadoPrimeiraPagina> {
  const resultado = await listarProdutos({
    busca: termoAtual || undefined,
    pagina: 1,
    itensPorPagina: ITENS_POR_PAGINA,
  });

  return {
    itens: resultado.itens,
    total: resultado.total,
    pagina: resultado.pagina,
    totalPaginas: resultado.totalPaginas,
  };
}

async function buscarDestaques(): Promise<Produto[]> {
  const resultado = await listarProdutos({
    emPromocao: true,
    pagina: 1,
    itensPorPagina: ITENS_DESTAQUE,
  });

  return resultado.itens;
}

function CartaoProduto({ produto, destaque = false }: { produto: Produto; destaque?: boolean }) {
  const { add, quantidadeDe } = useCart();
  const semEstoque = produto.quantidade <= 0;
  const limite = quantidadeDe(produto.id) >= produto.quantidade;

  return (
    <View style={[s.cardBase, destaque ? s.cardDestaque : s.card]}>
      <View>
        <View style={s.cardTopo}>
          <Text style={s.categoria} numberOfLines={1}>
            {produto.categoriaNome}
          </Text>
          {produto.emPromocao ? (
            <View style={s.promoTag}>
              <Text style={s.promoTagTxt}>PROMO</Text>
            </View>
          ) : null}
        </View>
        <Text style={s.nome} numberOfLines={2}>
          {produto.nome}
        </Text>
      </View>

      <View>
        <PrecoProduto produto={produto} tamanho={15} />
        <View style={s.estoqueLinha}>
          <Text
            style={[
              s.estoque,
              semEstoque
                ? { color: colors.danger }
                : produto.quantidade <= 5
                  ? { color: colors.warning }
                  : { color: colors.success },
            ]}>
            {produto.status}
          </Text>
          <Text style={s.estoqueQtd}>{produto.quantidade} un.</Text>
        </View>
      </View>

      <AddToCartButton onPress={() => add(produto)} disabled={semEstoque || limite} size={34} />
    </View>
  );
}

export default function Inicio() {
  const router = useRouter();
  const { colunas } = useGrade();

  const [busca, setBusca] = useState('');
  const [termo, setTermo] = useState('');
  const [categorias, setCategorias] = useState<OpcaoCategoria[]>([]);
  const [produtos, setProdutos] = useState<Produto[]>([]);
  const [destaques, setDestaques] = useState<Produto[]>([]);
  const [total, setTotal] = useState(0);
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(1);
  const [carregando, setCarregando] = useState(true);
  const [carregandoMais, setCarregandoMais] = useState(false);
  const [atualizando, setAtualizando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [tentativa, setTentativa] = useState(0);

  const requisicaoRef = useRef(0);

  useEffect(() => {
    const proximo = busca.trim();

    if (proximo === termo) return;

    const timer = setTimeout(() => {
      setAtualizando(false);
      setCarregandoMais(false);
      setCarregando(true);
      setErro(null);
      setTermo(proximo);
    }, 350);

    return () => clearTimeout(timer);
  }, [busca, termo]);

  useEffect(() => {
    let ativo = true;

    listarOpcoesCategorias()
      .then((lista) => {
        if (ativo) setCategorias(lista);
      })
      .catch(() => {
        // O catálogo continua útil mesmo se as categorias falharem.
      });

    return () => {
      ativo = false;
    };
  }, []);

  useEffect(() => {
    let ativo = true;
    const requisicao = ++requisicaoRef.current;

    const destaquesPromise: Promise<Produto[] | null> = termo
      ? Promise.resolve(null)
      : buscarDestaques().catch(() => []);

    Promise.all([buscarPrimeiraPagina(termo), destaquesPromise])
      .then(([resultado, listaDestaques]) => {
        if (!ativo || requisicao !== requisicaoRef.current) return;

        setProdutos(resultado.itens);
        setTotal(resultado.total);
        setPagina(resultado.pagina);
        setTotalPaginas(resultado.totalPaginas);

        if (listaDestaques !== null) setDestaques(listaDestaques);

        setCarregando(false);
      })
      .catch((falha) => {
        if (!ativo || requisicao !== requisicaoRef.current) return;

        setProdutos([]);
        setTotal(0);
        setErro(mensagemDeErro(falha));
        setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [termo, tentativa]);

  async function atualizar() {
    const requisicao = ++requisicaoRef.current;

    setAtualizando(true);
    setErro(null);

    try {
      const destaquesPromise: Promise<Produto[] | null> = termo
        ? Promise.resolve(null)
        : buscarDestaques().catch(() => null);

      const [resultado, listaCategorias, listaDestaques] = await Promise.all([
        buscarPrimeiraPagina(termo),
        listarOpcoesCategorias(),
        destaquesPromise,
      ]);

      if (requisicao !== requisicaoRef.current) return;

      setCategorias(listaCategorias);
      setProdutos(resultado.itens);
      setTotal(resultado.total);
      setPagina(resultado.pagina);
      setTotalPaginas(resultado.totalPaginas);

      if (listaDestaques !== null) setDestaques(listaDestaques);
    } catch (falha) {
      if (requisicao !== requisicaoRef.current) return;
      setErro(mensagemDeErro(falha));
    } finally {
      if (requisicao === requisicaoRef.current) {
        setAtualizando(false);
        setCarregando(false);
      }
    }
  }

  function tentarNovamente() {
    setErro(null);
    setAtualizando(false);
    setCarregandoMais(false);
    setCarregando(true);
    setTentativa((valor) => valor + 1);
  }

  async function carregarMais() {
    if (carregando || carregandoMais || pagina >= totalPaginas) return;

    const requisicao = requisicaoRef.current;

    setCarregandoMais(true);

    try {
      const filtro: FiltroProdutos = termo ? { busca: termo } : {};

      const resultado = await listarProdutos({
        ...filtro,
        pagina: pagina + 1,
        itensPorPagina: ITENS_POR_PAGINA,
      });

      if (requisicao !== requisicaoRef.current) return;

      setProdutos((atuais) => {
        const ids = new Set(atuais.map((produto) => produto.id));
        return [...atuais, ...resultado.itens.filter((produto) => !ids.has(produto.id))];
      });
      setPagina(resultado.pagina);
      setTotalPaginas(resultado.totalPaginas);
    } catch (falha) {
      if (requisicao !== requisicaoRef.current) return;
      setErro(mensagemDeErro(falha));
    } finally {
      setCarregandoMais(false);
    }
  }

  function tituloLista() {
    if (termo) return `Resultados para "${termo}"`;
    return 'Todos os produtos';
  }

  return (
    <ScreenLayout title="Início" subtitle="Escolha seus produtos favoritos">
      <FlatList
        key={`grade-${colunas}`}
        style={s.container}
        data={produtos}
        keyExtractor={(produto) => String(produto.id)}
        numColumns={colunas}
        columnWrapperStyle={{ gap: 12 }}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={atualizando} onRefresh={atualizar} tintColor={colors.primary} />
        }
        ListHeaderComponent={
          <View>
            <View style={s.buscaBox}>
              <Ionicons name="search" size={18} color={colors.muted} />
              <TextInput
                style={s.busca}
                placeholder="Buscar produtos..."
                placeholderTextColor={colors.muted}
                value={busca}
                onChangeText={setBusca}
                autoCapitalize="none"
                returnKeyType="search"
              />
              {busca ? (
                <TouchableOpacity onPress={() => setBusca('')} hitSlop={8}>
                  <Ionicons name="close-circle" size={18} color={colors.muted} />
                </TouchableOpacity>
              ) : null}
            </View>

            <FlatList
              horizontal
              data={categorias}
              keyExtractor={(categoria) => String(categoria.id)}
              showsHorizontalScrollIndicator={false}
              contentContainerStyle={s.categorias}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={s.cat}
                  onPress={() => router.push(`/categoria/${item.id}`)}>
                  <Ionicons name="pricetag" size={22} color={colors.primary} />
                  <Text style={s.catTxt} numberOfLines={1}>
                    {item.nome}
                  </Text>
                </TouchableOpacity>
              )}
            />

            {!termo && destaques.length > 0 ? (
              <>
                <View style={s.secaoLinha}>
                  <Text style={s.secao}>Em destaque</Text>
                  <Text style={s.contagem}>
                    {destaques.length} {destaques.length === 1 ? 'produto' : 'produtos'}
                  </Text>
                </View>

                <FlatList
                  horizontal
                  data={destaques}
                  keyExtractor={(produto) => String(produto.id)}
                  showsHorizontalScrollIndicator={false}
                  contentContainerStyle={s.destaquesLista}
                  renderItem={({ item }) => <CartaoProduto produto={item} destaque />}
                />
              </>
            ) : null}

            <View style={s.secaoLinha}>
              <Text style={s.secao}>{tituloLista()}</Text>
              {!carregando && !erro ? (
                <Text style={s.contagem}>
                  {total} {total === 1 ? 'produto' : 'produtos'}
                </Text>
              ) : null}
            </View>

            {erro ? (
              <View style={s.erroBox}>
                <Ionicons name="cloud-offline-outline" size={20} color={colors.danger} />
                <View style={s.erroTextoBox}>
                  <Text style={s.erroTitulo}>Não foi possível carregar os produtos</Text>
                  <Text style={s.erroTexto}>{erro}</Text>
                </View>
                <TouchableOpacity onPress={tentarNovamente}>
                  <Text style={s.erroAcao}>Tentar</Text>
                </TouchableOpacity>
              </View>
            ) : null}
          </View>
        }
        ListEmptyComponent={
          carregando ? (
            <View style={s.vazio}>
              <ActivityIndicator color={colors.primary} size="large" />
            </View>
          ) : erro ? null : (
            <View style={s.vazio}>
              <Ionicons name="search-outline" size={42} color={colors.muted} />
              <Text style={s.vazioTitulo}>
                {termo ? 'Nenhum produto encontrado' : 'Nenhum produto disponível'}
              </Text>
              <Text style={s.vazioTexto}>
                {termo
                  ? 'Tente outro termo de busca.'
                  : 'Novos produtos aparecerão aqui assim que forem cadastrados.'}
              </Text>
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
        renderItem={({ item }) => <CartaoProduto produto={item} />}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  buscaBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 10,
    paddingHorizontal: 12,
    marginBottom: 14,
    backgroundColor: colors.bg,
  },
  busca: { flex: 1, paddingVertical: 11, color: colors.text },
  categorias: { gap: 10, paddingBottom: 4 },
  cat: {
    alignItems: 'center',
    backgroundColor: colors.card,
    paddingVertical: 12,
    paddingHorizontal: 14,
    borderRadius: 10,
    width: 92,
  },
  catTxt: { fontSize: 11, marginTop: 4, color: colors.text },
  secaoLinha: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 18,
    marginBottom: 12,
    gap: 8,
  },
  secao: { fontSize: 16, fontWeight: '700', color: colors.text, flex: 1 },
  contagem: { fontSize: 12, color: colors.muted },
  destaquesLista: { gap: 12, paddingBottom: 4 },
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

  cardBase: {
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 12,
    marginBottom: 12,
    minHeight: 175,
    justifyContent: 'space-between',
  },
  card: { flex: 1 },
  cardDestaque: { flexGrow: 0, flexShrink: 0, flexBasis: 'auto', width: 168 },
  cardTopo: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 6,
    marginBottom: 4,
  },
  categoria: { fontSize: 10, color: colors.muted, textTransform: 'uppercase', flex: 1 },
  promoTag: {
    backgroundColor: colors.primary,
    borderRadius: 999,
    paddingHorizontal: 6,
    paddingVertical: 2,
  },
  promoTagTxt: { color: '#fff', fontSize: 8, fontWeight: '800' },
  nome: { fontWeight: '600', color: colors.text, minHeight: 36 },
  estoqueLinha: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 4,
    marginTop: 4,
    marginBottom: 8,
  },
  estoque: { fontSize: 10, fontWeight: '700' },
  estoqueQtd: { fontSize: 10, color: colors.muted },

  vazio: { alignItems: 'center', paddingVertical: 48, gap: 6 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: colors.text, marginTop: 8 },
  vazioTexto: { color: colors.muted },
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
