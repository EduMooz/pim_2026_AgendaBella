namespace AgendaBella.Api.Seguranca;

// Espelho da seção "Jwt" do appsettings.json. O Program.cs lê a seção e preenche este objeto.
public class ConfiguracaoJwt
{
    public string Chave { get; set; } = string.Empty;     // segredo usado para ASSINAR o token (mínimo 32 caracteres)
    public string Emissor { get; set; } = string.Empty;   // quem emitiu o token (a Api)
    public string Publico { get; set; } = string.Empty;   // para quem o token foi emitido (o app)
    public int ExpiracaoHoras { get; set; } = 8;
}
