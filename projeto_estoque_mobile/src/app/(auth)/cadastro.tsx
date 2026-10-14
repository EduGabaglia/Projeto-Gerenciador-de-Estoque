import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert } from 'react-native';
import { Link } from 'expo-router';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

export default function Cadastro() {
  const { signIn } = useAuth();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');

  async function cadastrar() {
    if (!nome || !email || !senha) {
      Alert.alert('Atenção', 'Preencha todos os campos');
      return;
    }
    // MOCK: quando a API estiver pronta, aqui vai o api.post('/auth/cadastro', ...)
    await signIn(email, senha);
  }

  return (
    <View style={s.container}>
      <Text style={s.titulo}>Criar conta</Text>

      <TextInput style={s.input} placeholder="Nome" value={nome} onChangeText={setNome} />
      <TextInput style={s.input} placeholder="E-mail" autoCapitalize="none" keyboardType="email-address" value={email} onChangeText={setEmail} />
      <TextInput style={s.input} placeholder="Senha" secureTextEntry value={senha} onChangeText={setSenha} />

      <TouchableOpacity style={s.botao} onPress={cadastrar}>
        <Text style={s.botaoTxt}>Cadastrar</Text>
      </TouchableOpacity>

      <Text style={s.muted}>
        Já tem uma conta? <Link href="/(auth)/login" style={s.link}>Entrar</Link>
      </Text>
    </View>
  );
}

const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 24, justifyContent: 'center' },
  titulo: { fontSize: 22, fontWeight: '700', textAlign: 'center', marginBottom: 24, color: colors.text },
  input: { borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 12, marginBottom: 12 },
  botao: { backgroundColor: colors.primary, padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 4 },
  botaoTxt: { color: '#fff', fontWeight: '600' },
  muted: { textAlign: 'center', color: colors.muted, marginTop: 16 },
  link: { color: colors.primary },
});