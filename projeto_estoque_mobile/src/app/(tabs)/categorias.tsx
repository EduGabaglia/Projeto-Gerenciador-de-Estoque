import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
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
import { ScreenLayout } from '@/components/screen-layout';
import type { Categoria } from '@/data/catalogo';
import { mensagemDeErro } from '@/services/api';
import { listarCategorias } from '@/services/catalogo';
import { colors } from '@/theme';

export default function Categorias() {
  const router = useRouter();

  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [atualizando, setAtualizando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    let ativo = true;

    listarCategorias()
      .then((lista) => {
        if (!ativo) return;
        setCategorias(lista);
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
  }, [tentativa]);

  async function atualizar() {
    setAtualizando(true);
    setErro(null);

    try {
      setCategorias(await listarCategorias());
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setAtualizando(false);
      setCarregando(false);
    }
  }

  function tentarNovamente() {
    setErro(null);
    setCarregando(true);
    setTentativa((valor) => valor + 1);
  }

  return (
    <ScreenLayout title="Categorias" subtitle="Navegue por todas as seções">
      <FlatList
        style={s.container}
        data={categorias}
        keyExtractor={(categoria) => String(categoria.id)}
        contentContainerStyle={s.lista}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={atualizando} onRefresh={atualizar} tintColor={colors.primary} />
        }
        ListHeaderComponent={
          erro ? (
            <View style={s.erroBox}>
              <Ionicons name="cloud-offline-outline" size={20} color={colors.danger} />
              <View style={s.erroTextoBox}>
                <Text style={s.erroTitulo}>Não foi possível carregar as categorias</Text>
                <Text style={s.erroTexto}>{erro}</Text>
              </View>
              <TouchableOpacity onPress={tentarNovamente}>
                <Text style={s.erroAcao}>Tentar</Text>
              </TouchableOpacity>
            </View>
          ) : null
        }
        ListEmptyComponent={
          carregando ? (
            <View style={s.vazio}>
              <ActivityIndicator color={colors.primary} size="large" />
            </View>
          ) : erro ? null : (
            <View style={s.vazio}>
              <Ionicons name="grid-outline" size={44} color={colors.muted} />
              <Text style={s.vazioTitulo}>Nenhuma categoria cadastrada</Text>
            </View>
          )
        }
        renderItem={({ item }) => (
          <TouchableOpacity
            style={s.linha}
            activeOpacity={0.8}
            onPress={() => router.push(`/categoria/${item.id}`)}>
            <View style={s.icone}>
              <Ionicons name="pricetag" size={20} color={colors.primary} />
            </View>
            <View style={s.textoBox}>
              <Text style={s.nome}>{item.nome}</Text>
              <Text style={s.detalhe}>
                {item.produtosVinculados}{' '}
                {item.produtosVinculados === 1 ? 'produto' : 'produtos'}
              </Text>
            </View>
            <Ionicons name="chevron-forward" size={18} color={colors.muted} />
          </TouchableOpacity>
        )}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 24 },
  linha: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.card,
    borderRadius: 10,
    padding: 14,
    marginBottom: 10,
    gap: 12,
  },
  icone: {
    width: 38,
    height: 38,
    borderRadius: 10,
    backgroundColor: colors.tabBg,
    alignItems: 'center',
    justifyContent: 'center',
  },
  textoBox: { flex: 1, minWidth: 0 },
  nome: { fontWeight: '600', color: colors.text },
  detalhe: { fontSize: 11, color: colors.muted, marginTop: 2 },
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
});
