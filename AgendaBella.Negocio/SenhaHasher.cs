using System.Security.Cryptography;

namespace AgendaBella.Negocio;

// Transforma a senha em um "hash": um texto embaralhado que NÃO volta a ser a senha.
// O banco guarda só o hash. No login, o sistema calcula o hash da senha digitada
// e compara com o guardado.
//
// Formato guardado:  PBKDF2$<iterações>$<sal em Base64>$<hash em Base64>
//   - sal: texto aleatório, diferente para cada senha (duas pessoas com a mesma
//     senha ficam com hashes diferentes);
//   - iterações: quantas vezes o cálculo é repetido (deixa o ataque por força bruta lento).
// O número de iterações vai dentro do texto, então pode ser aumentado no futuro
// sem invalidar as senhas antigas. 600.000 iterações com SHA-256 é o valor
// recomendado pelo OWASP (Password Storage Cheat Sheet).
public static class SenhaHasher
{
    private const int Iteracoes = 600_000;
    private const int TamanhoSal = 16;
    private const int TamanhoHash = 32;

    public static string Gerar(string senha)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanhoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return $"PBKDF2${Iteracoes}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string armazenado)
    {
        var partes = armazenado.Split('$');
        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteracoes) || iteracoes <= 0)
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            if (sal.Length == 0 || esperado.Length == 0) return false;
            var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            // Comparação em tempo constante: não revela, pelo tempo de resposta, quantos bytes batem.
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
