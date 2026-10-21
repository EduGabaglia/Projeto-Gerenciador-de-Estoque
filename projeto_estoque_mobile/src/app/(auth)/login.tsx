import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Link } from 'expo-router';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

export default function Login() {
  const { signIn } = useAuth();
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
    <View style={s.container}>
      <Ionicons name="cart-outline" size={48} color={colors.primary} style={{ alignSelf: 'center' }} />
      <Text style={s.titulo}>Minha Loja</Text>
      <Text style={s.sub}>Bem-vindo!</Text>
      <Text style={s.muted}>Faça login para continuar</Text>

      <TextInput style={s.input} placeholder="E-mail" autoCapitalize="none" keyboardType="email-address" value={email} onChangeText={setEmail} />
      <TextInput style={s.input} placeholder="Senha" secureTextEntry value={senha} onChangeText={setSenha} />

      <TouchableOpacity style={s.botao} onPress={entrar}>
        <Text style={s.botaoTxt}>Entrar</Text>
      </TouchableOpacity>

      <Link href="/(auth)/cadastro" style={s.link}>Criar conta</Link>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 24, justifyContent: 'center' },
  titulo: { fontSize: 24, fontWeight: '700', textAlign: 'center', marginTop: 8, color: colors.text },
  sub: { fontSize: 16, fontWeight: '600', textAlign: 'center', marginTop: 16 },
  muted: { textAlign: 'center', color: colors.muted, marginBottom: 24 },
  input: { borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 12, marginBottom: 12 },
  botao: { backgroundColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 4 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
  link: { textAlign: 'center', color: colors.primary, marginTop: 16 },
});