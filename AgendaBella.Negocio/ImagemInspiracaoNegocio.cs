namespace AgendaBella.Negocio;

// Recebe a foto de inspiração (o "modelo de unha" que a cliente quer) e a grava em disco.
// Não depende de ASP.NET: recebe um Stream comum. Quem chama (Web ou Api) diz em qual pasta gravar.
// No banco vai apenas o CAMINHO RELATIVO (ex.: inspiracao/3f2a...jpg).
public class ImagemInspiracaoNegocio
{
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024;     // 5 MB
    private const string Subpasta = "inspiracao";
    private static readonly string[] Extensoes = { ".jpg", ".jpeg", ".png", ".webp" };

    public async Task<string> SalvarAsync(Stream conteudo, string nomeOriginal, long tamanho, string pastaRaiz)
    {
        var extensao = Path.GetExtension(nomeOriginal ?? string.Empty).ToLowerInvariant();

        if (tamanho <= 0) throw new RegraDeNegocioException("Arquivo vazio.");
        if (tamanho > TamanhoMaximoBytes) throw new RegraDeNegocioException("A imagem pode ter no máximo 5 MB.");
        if (!Extensoes.Contains(extensao)) throw new RegraDeNegocioException("Envie uma imagem JPG, PNG ou WEBP.");

        // Não basta confiar na extensão: confere os primeiros bytes do arquivo ("assinatura").
        var cabecalho = new byte[12];
        var lidos = 0;
        while (lidos < cabecalho.Length)
        {
            var n = await conteudo.ReadAsync(cabecalho.AsMemory(lidos, cabecalho.Length - lidos));
            if (n == 0) break;
            lidos += n;
        }
        if (!AssinaturaValida(cabecalho, lidos))
            throw new RegraDeNegocioException("O arquivo enviado não é uma imagem válida.");

        // Nome novo e aleatório: evita sobrescrever arquivos e ignora o nome enviado pelo usuário.
        var nomeFinal = $"{Guid.NewGuid():N}{extensao}";
        var relativo = $"{Subpasta}/{nomeFinal}";
        var pastaDestino = Path.Combine(pastaRaiz, Subpasta);
        Directory.CreateDirectory(pastaDestino);

        await using var arquivo = new FileStream(Path.Combine(pastaDestino, nomeFinal), FileMode.CreateNew, FileAccess.Write);
        await arquivo.WriteAsync(cabecalho.AsMemory(0, lidos));
        await conteudo.CopyToAsync(arquivo);
        return relativo;
    }

    // Apaga um arquivo gravado (usado quando o agendamento falha depois do upload).
    public void Remover(string? caminhoRelativo, string pastaRaiz)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo)) return;

        var raiz = Path.GetFullPath(pastaRaiz);
        var completo = Path.GetFullPath(Path.Combine(raiz, caminhoRelativo));
        // Segurança: só apaga se o arquivo estiver DENTRO da pasta de uploads.
        if (!completo.StartsWith(raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(completo)) File.Delete(completo);
    }

    private static bool AssinaturaValida(byte[] b, int lidos)
    {
        if (lidos >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return true;                           // JPEG
        if (lidos >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return true;           // PNG
        if (lidos >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F' &&
                           b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P') return true;            // WEBP
        return false;
    }
}
