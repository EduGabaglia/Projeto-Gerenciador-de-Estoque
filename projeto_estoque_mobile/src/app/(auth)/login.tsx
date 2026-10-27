import { Ionicons } from '@expo/vector-icons';
import { Link } from 'expo-router';
import { useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuth } from '@/contexts/AuthContext';
import { mensagemDeErro } from '@/services/api';
import { colors } from '@/theme';

export default function Login() {
  const { signIn } = useAuth();
  const insets = useSafeAreaInsets();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [entrando, setEntrando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  async function entrar() {
    if (entrando) return;

    if (!email.trim() || !senha) {
      setErro('Preencha o e-mail e a senha.');
      return;
    }

    setEntrando(true);
    setErro(null);

    try {
      await signIn(email, senha);
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setEntrando(false);
    }
  }

  return (
    <KeyboardAvoidingView
      style={s.fundo}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView
        contentContainerStyle={[
          s.container,
          { paddingTop: insets.top + 24, paddingBottom: insets.bottom + 24 },
        ]}
        keyboardShouldPersistTaps="handled">
        <View style={s.card}>
          <Ionicons name="cart-outline" size={48} color={colors.primary} style={s.icone} />
          <Text style={s.titulo}>Minha Loja</Text>
          <Text style={s.sub}>Bem-vindo!</Text>
          <Text style={s.muted}>Faça login para continuar</Text>

          <TextInput
            style={s.input}
            placeholder="E-mail"
            placeholderTextColor={colors.muted}
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            value={email}
            editable={!entrando}
            onChangeText={setEmail}
          />
          <TextInput
            style={s.input}
            placeholder="Senha"
            placeholderTextColor={colors.muted}
            secureTextEntry
            autoCapitalize="none"
            autoComplete="current-password"
            value={senha}
            editable={!entrando}
            onChangeText={setSenha}
            onSubmitEditing={() => void entrar()}
            returnKeyType="done"
          />

          {erro ? (
            <View style={s.erroBox}>
              <Ionicons name="alert-circle" size={17} color={colors.danger} />
              <Text style={s.erroTxt}>{erro}</Text>
            </View>
          ) : null}

          <TouchableOpacity
            style={[s.botao, entrando && s.botaoDesabilitado]}
            activeOpacity={0.85}
            disabled={entrando}
            onPress={() => void entrar()}>
            {entrando ? (
              <ActivityIndicator color="#fff" size="small" />
            ) : (
              <Text style={s.botaoTxt}>Entrar</Text>
            )}
          </TouchableOpacity>

          <Link href="/(auth)/cadastro" style={s.link}>
            Criar conta
          </Link>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const s = StyleSheet.create({
  fundo: { flex: 1, backgroundColor: colors.tabBg },
  container: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 24,
  },
  card: {
    width: '100%',
    maxWidth: 420,
    backgroundColor: colors.bg,
    borderRadius: 16,
    padding: 24,
    shadowColor: '#0F172A',
    shadowOpacity: 0.08,
    shadowRadius: 16,
    shadowOffset: { width: 0, height: 6 },
    elevation: 3,
  },
  icone: { alignSelf: 'center' },
  titulo: { fontSize: 24, fontWeight: '700', textAlign: 'center', marginTop: 8, color: colors.text },
  sub: { fontSize: 16, fontWeight: '600', textAlign: 'center', marginTop: 16, color: colors.text },
  muted: { textAlign: 'center', color: colors.muted, marginBottom: 24 },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    padding: 12,
    marginBottom: 12,
    color: colors.text,
    backgroundColor: colors.bg,
  },
  erroBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: colors.dangerBg,
    borderRadius: 8,
    padding: 10,
    marginBottom: 8,
  },
  erroTxt: { color: colors.danger, fontSize: 12, flex: 1 },
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 4,
  },
  botaoDesabilitado: { opacity: 0.7 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
  link: { textAlign: 'center', color: colors.primary, marginTop: 16, fontWeight: '600' },
});
