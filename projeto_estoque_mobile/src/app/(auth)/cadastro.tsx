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

export default function Cadastro() {
  const { signUp } = useAuth();
  const insets = useSafeAreaInsets();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmacao, setConfirmacao] = useState('');
  const [cadastrando, setCadastrando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  async function cadastrar() {
    if (cadastrando) return;

    if (!nome.trim() || !email.trim() || !senha || !confirmacao) {
      setErro('Preencha todos os campos.');
      return;
    }

    if (senha.length < 6) {
      setErro('A senha precisa ter pelo menos 6 caracteres.');
      return;
    }

    if (senha !== confirmacao) {
      setErro('A confirmação não confere com a senha.');
      return;
    }

    setCadastrando(true);
    setErro(null);

    try {
      await signUp(nome, email, senha);
    } catch (falha) {
      setErro(mensagemDeErro(falha));
    } finally {
      setCadastrando(false);
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
          <Text style={s.titulo}>Criar conta</Text>

          <TextInput
            style={s.input}
            placeholder="Nome"
            placeholderTextColor={colors.muted}
            autoCapitalize="words"
            autoComplete="name"
            value={nome}
            editable={!cadastrando}
            onChangeText={setNome}
          />
          <TextInput
            style={s.input}
            placeholder="E-mail"
            placeholderTextColor={colors.muted}
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            value={email}
            editable={!cadastrando}
            onChangeText={setEmail}
          />
          <TextInput
            style={s.input}
            placeholder="Senha (mínimo 6 caracteres)"
            placeholderTextColor={colors.muted}
            secureTextEntry
            autoCapitalize="none"
            autoComplete="new-password"
            value={senha}
            editable={!cadastrando}
            onChangeText={setSenha}
          />
          <TextInput
            style={s.input}
            placeholder="Confirmar senha"
            placeholderTextColor={colors.muted}
            secureTextEntry
            autoCapitalize="none"
            autoComplete="new-password"
            value={confirmacao}
            editable={!cadastrando}
            onChangeText={setConfirmacao}
            onSubmitEditing={() => void cadastrar()}
            returnKeyType="done"
          />

          {erro ? (
            <View style={s.erroBox}>
              <Ionicons name="alert-circle" size={17} color={colors.danger} />
              <Text style={s.erroTxt}>{erro}</Text>
            </View>
          ) : null}

          <TouchableOpacity
            style={[s.botao, cadastrando && s.botaoDesabilitado]}
            activeOpacity={0.85}
            disabled={cadastrando}
            onPress={() => void cadastrar()}>
            {cadastrando ? (
              <ActivityIndicator color="#fff" size="small" />
            ) : (
              <Text style={s.botaoTxt}>Cadastrar</Text>
            )}
          </TouchableOpacity>

          <Text style={s.muted}>
            Já tem uma conta?{' '}
            <Link href="/(auth)/login" style={s.link}>
              Entrar
            </Link>
          </Text>
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
  titulo: {
    fontSize: 22,
    fontWeight: '700',
    textAlign: 'center',
    marginBottom: 24,
    color: colors.text,
  },
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
  muted: { textAlign: 'center', color: colors.muted, marginTop: 16 },
  link: { color: colors.primary, fontWeight: '600' },
});
