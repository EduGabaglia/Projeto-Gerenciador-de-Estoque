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
import { Ionicons } from '@expo/vector-icons';
import { Link } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

export default function Login() {
  const { signIn } = useAuth();
  const insets = useSafeAreaInsets();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');

  async function entrar() {
    try {
      await signIn(email, senha);
    } catch {
      Alert.alert('Erro', 'E-mail ou senha inválidos');
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

          <TouchableOpacity style={s.botao} activeOpacity={0.85} onPress={entrar}>
            <Text style={s.botaoTxt}>Entrar</Text>
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
  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 4,
  },
  botaoTxt: { color: '#fff', fontWeight: '600' },
  link: { textAlign: 'center', color: colors.primary, marginTop: 16, fontWeight: '600' },
});
