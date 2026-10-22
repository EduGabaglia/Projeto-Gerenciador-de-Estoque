import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import { Link } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

export default function Cadastro() {
  const { signIn } = useAuth();
  const insets = useSafeAreaInsets();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');

  async function cadastrar() {
    if (!nome || !email || !senha) {
      Alert.alert('Atenção', 'Preencha todos os campos');
      return;
    }
    // MOCK: quando a API estiver pronta, aqui vai o api.post('/auth/cadastro', ...)
    await signIn(email, senha, nome);
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
            value={nome}
            onChangeText={setNome}
          />
          <TextInput
            style={s.input}
            placeholder="E-mail"
            placeholderTextColor={colors.muted}
            autoCapitalize="none"
            keyboardType="email-address"
            value={email}
            onChangeText={setEmail}
          />
          <TextInput
            style={s.input}
            placeholder="Senha"
            placeholderTextColor={colors.muted}
            secureTextEntry
            value={senha}
            onChangeText={setSenha}
          />

          <TouchableOpacity style={s.botao} activeOpacity={0.85} onPress={cadastrar}>
            <Text style={s.botaoTxt}>Cadastrar</Text>
          </TouchableOpacity>

          <Text style={s.muted}>
            Já tem uma conta? <Link href="/(auth)/login" style={s.link}>Entrar</Link>
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
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 4,
  },
  botaoTxt: { color: '#fff', fontWeight: '600' },
  muted: { textAlign: 'center', color: colors.muted, marginTop: 16 },
  link: { color: colors.primary, fontWeight: '600' },
});
