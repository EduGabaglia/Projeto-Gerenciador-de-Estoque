import { Ionicons } from '@expo/vector-icons';
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
import { ConfirmDialog } from '@/components/confirm-dialog';
import { ScreenLayout } from '@/components/screen-layout';
import { useAuth } from '@/contexts/AuthContext';
import { colors } from '@/theme';

type Feedback = { tipo: 'ok' | 'erro'; texto: string } | null;

export default function Conta() {
  const { usuario, atualizarPerfil, alterarSenha, excluirConta } = useAuth();

  const [nomeEditado, setNomeEditado] = useState<string | null>(null);
  const [telefoneEditado, setTelefoneEditado] = useState<string | null>(null);
  // Enquanto o usuário não digita, o campo acompanha o perfil — inclusive se
  // ele terminar de carregar depois da tela abrir.
  const nome = nomeEditado ?? usuario?.nome ?? 'Cliente';
  const telefone = telefoneEditado ?? usuario?.telefone ?? '';
  const [senhaAtual, setSenhaAtual] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmacao, setConfirmacao] = useState('');

  const [salvando, setSalvando] = useState(false);
  const [feedback, setFeedback] = useState<Feedback>(null);
  const [excluindo, setExcluindo] = useState(false);
  const [dialogoExclusao, setDialogoExclusao] = useState(false);

  async function salvar() {
    setFeedback(null);

    if (!nome.trim()) {
      setFeedback({ tipo: 'erro', texto: 'Informe seu nome.' });
      return;
    }

    const querTrocarSenha = Boolean(senhaAtual || novaSenha || confirmacao);

    if (querTrocarSenha) {
      if (!senhaAtual || !novaSenha || !confirmacao) {
        setFeedback({ tipo: 'erro', texto: 'Preencha os três campos de senha.' });
        return;
      }
      if (novaSenha.length < 6) {
        setFeedback({ tipo: 'erro', texto: 'A nova senha precisa ter pelo menos 6 caracteres.' });
        return;
      }
      if (novaSenha === senhaAtual) {
        setFeedback({ tipo: 'erro', texto: 'A nova senha precisa ser diferente da atual.' });
        return;
      }
      if (novaSenha !== confirmacao) {
        setFeedback({ tipo: 'erro', texto: 'A confirmação não confere com a nova senha.' });
        return;
      }
    }

    setSalvando(true);

    try {
      // Troca a senha primeiro: se a senha atual estiver errada, nada é salvo.
      if (querTrocarSenha) await alterarSenha(senhaAtual, novaSenha);
      await atualizarPerfil({ nome, telefone });

      setNomeEditado(null);
      setTelefoneEditado(null);
      setSenhaAtual('');
      setNovaSenha('');
      setConfirmacao('');
      setFeedback({ tipo: 'ok', texto: 'Dados atualizados com sucesso.' });
    } catch (erro) {
      setFeedback({
        tipo: 'erro',
        texto: erro instanceof Error ? erro.message : 'Não foi possível salvar as alterações.',
      });
    } finally {
      setSalvando(false);
    }
  }

  async function confirmarExclusao() {
    setExcluindo(true);
    setFeedback(null);

    try {
      await excluirConta();
      setDialogoExclusao(false);
    } catch (erro) {
      setFeedback({
        tipo: 'erro',
        texto: erro instanceof Error ? erro.message : 'Não foi possível excluir a conta.',
      });
      setDialogoExclusao(false);
    } finally {
      setExcluindo(false);
    }
  }

  return (
    <ScreenLayout
      title="Gerenciar conta"
      subtitle="Seus dados e segurança"
      mostrarVoltar
      voltarPara="/(tabs)/perfil">
      <KeyboardAvoidingView
        style={s.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView
          style={s.flex}
          contentContainerStyle={s.lista}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}>
          <Text style={s.secao}>Dados pessoais</Text>

          <View style={s.card}>
            <Text style={s.label}>Nome</Text>
            <TextInput
              style={s.input}
              value={nome}
              onChangeText={(texto) => setNomeEditado(texto)}
              placeholder="Seu nome"
              placeholderTextColor={colors.muted}
              autoCapitalize="words"
            />

            <Text style={s.label}>Telefone</Text>
            <TextInput
              style={s.input}
              value={telefone}
              onChangeText={setTelefoneEditado}
              placeholder="(00) 00000-0000"
              placeholderTextColor={colors.muted}
              keyboardType="phone-pad"
              maxLength={15}
            />

            <Text style={s.label}>E-mail</Text>
            <View style={[s.input, s.inputBloqueado]}>
              <Text style={s.inputBloqueadoTxt} numberOfLines={1}>
                {usuario?.email ?? '—'}
              </Text>
              <Ionicons name="lock-closed-outline" size={16} color={colors.muted} />
            </View>
            <Text style={s.ajuda}>O e-mail de acesso não pode ser alterado.</Text>
          </View>

          <Text style={s.secao}>Alterar senha</Text>

          <View style={s.card}>
            <Text style={s.label}>Senha atual</Text>
            <TextInput
              style={s.input}
              value={senhaAtual}
              onChangeText={setSenhaAtual}
              placeholder="••••••"
              placeholderTextColor={colors.muted}
              secureTextEntry
              autoCapitalize="none"
            />

            <Text style={s.label}>Nova senha</Text>
            <TextInput
              style={s.input}
              value={novaSenha}
              onChangeText={setNovaSenha}
              placeholder="Mínimo de 6 caracteres"
              placeholderTextColor={colors.muted}
              secureTextEntry
              autoCapitalize="none"
            />

            <Text style={s.label}>Confirmar nova senha</Text>
            <TextInput
              style={s.input}
              value={confirmacao}
              onChangeText={setConfirmacao}
              placeholder="Repita a nova senha"
              placeholderTextColor={colors.muted}
              secureTextEntry
              autoCapitalize="none"
            />

            <Text style={s.ajuda}>Deixe os campos vazios para manter a senha atual.</Text>
          </View>

          {feedback ? (
            <View
              style={[
                s.feedback,
                { backgroundColor: feedback.tipo === 'ok' ? colors.successBg : colors.dangerBg },
              ]}>
              <Ionicons
                name={feedback.tipo === 'ok' ? 'checkmark-circle' : 'alert-circle'}
                size={18}
                color={feedback.tipo === 'ok' ? colors.success : colors.danger}
              />
              <Text
                style={[
                  s.feedbackTxt,
                  { color: feedback.tipo === 'ok' ? colors.success : colors.danger },
                ]}>
                {feedback.texto}
              </Text>
            </View>
          ) : null}

          <TouchableOpacity
            style={[s.botao, salvando && s.botaoDesabilitado]}
            activeOpacity={0.85}
            disabled={salvando}
            onPress={salvar}>
            {salvando ? (
              <ActivityIndicator color="#fff" size="small" />
            ) : (
              <Text style={s.botaoTxt}>Salvar alterações</Text>
            )}
          </TouchableOpacity>

          <Text style={s.secao}>Zona de perigo</Text>

          <View style={[s.card, s.cardPerigo]}>
            <Text style={s.perigoTitulo}>Excluir conta</Text>
            <Text style={s.perigoTexto}>
              Sua conta será desativada e você perderá o acesso. O histórico de pedidos é
              mantido no sistema e não é possível desfazer a exclusão.
            </Text>

            <TouchableOpacity
              style={s.botaoPerigo}
              activeOpacity={0.85}
              onPress={() => setDialogoExclusao(true)}>
              <Ionicons name="trash-outline" size={17} color={colors.danger} />
              <Text style={s.botaoPerigoTxt}>Excluir minha conta</Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>

      <ConfirmDialog
        visivel={dialogoExclusao}
        titulo="Excluir conta"
        mensagem="Sua conta será desativada e você voltará para a tela de login. O histórico de pedidos será mantido no sistema. Deseja continuar?"
        confirmar="Excluir conta"
        cancelar="Manter conta"
        destrutivo
        carregando={excluindo}
        onConfirmar={confirmarExclusao}
        onCancelar={() => setDialogoExclusao(false)}
      />
    </ScreenLayout>
  );
}

const s = StyleSheet.create({
  flex: { flex: 1, backgroundColor: colors.bg },
  lista: { padding: 16, paddingBottom: 40 },

  secao: { fontSize: 14, fontWeight: '700', color: colors.text, marginTop: 12, marginBottom: 8 },
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    padding: 14,
  },
  label: { fontSize: 12, fontWeight: '600', color: colors.muted, marginBottom: 6 },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 11,
    color: colors.text,
    backgroundColor: colors.bg,
    marginBottom: 12,
    fontSize: 14,
  },
  inputBloqueado: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  inputBloqueadoTxt: { flex: 1, color: colors.muted, fontSize: 14 },
  ajuda: { fontSize: 11, color: colors.muted, marginTop: -4 },

  feedback: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    borderRadius: 10,
    padding: 12,
    marginTop: 12,
  },
  feedbackTxt: { flex: 1, fontSize: 13, fontWeight: '600' },

  botao: {
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 14,
  },
  botaoDesabilitado: { opacity: 0.7 },
  botaoTxt: { color: '#fff', fontWeight: '700' },

  cardPerigo: { borderWidth: 1, borderColor: colors.dangerBg },
  perigoTitulo: { fontSize: 14, fontWeight: '700', color: colors.danger },
  perigoTexto: { fontSize: 12, color: colors.muted, marginTop: 4, lineHeight: 17 },
  botaoPerigo: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    borderWidth: 1,
    borderColor: colors.danger,
    borderRadius: 10,
    padding: 12,
    marginTop: 12,
    backgroundColor: colors.dangerBg,
  },
  botaoPerigoTxt: { color: colors.danger, fontWeight: '700', fontSize: 13 },
});
