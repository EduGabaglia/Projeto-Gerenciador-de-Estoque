using System.Security.Cryptography;

namespace projeto_estoque_api.Common;

/// <summary>
/// Hash de senhas com PBKDF2-SHA256. O formato persistido é
/// <c>pbkdf2$iteracoes$saltBase64$hashBase64</c>, que cabe no
/// <c>SenhaHash varchar(255)</c> da tabela Usuario.
/// </summary>
public static class SenhaHasher
{
    private const string Prefixo = "pbkdf2";
    private const int Iteracoes = 100_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int IteracoesMaximas = 1_000_000;

    public static string Criar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha,
            salt,
            Iteracoes,
            HashAlgorithmName.SHA256,
            TamanhoHash);

        return $"{Prefixo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string? hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(hashArmazenado))
            return false;

        var partes = hashArmazenado.Split('$');

        if (partes.Length != 4 ||
            partes[0] != Prefixo ||
            !int.TryParse(partes[1], out var iteracoes) ||
            iteracoes is < 1 or > IteracoesMaximas)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);

            if (salt.Length == 0 || esperado.Length == 0)
                return false;

            var calculado = Rfc2898DeriveBytes.Pbkdf2(
                senha,
                salt,
                iteracoes,
                HashAlgorithmName.SHA256,
                esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
