using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgendaBella.Modelos.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace AgendaBella.Api.Seguranca;

// Gera o TOKEN JWT entregue ao app Flutter depois do login.
//
// O que é um JWT: um texto em 3 partes separadas por ponto  ->  cabeçalho.conteúdo.assinatura
//   - o conteúdo (as "claims") diz QUEM é a pessoa: id, nome, e-mail e perfil;
//   - a assinatura é calculada com a Chave secreta do appsettings.json.
// O app guarda o token e o envia em toda requisição no cabeçalho:
//     Authorization: Bearer <token>
// A Api confere a assinatura; se alguém alterar o conteúdo, a assinatura não bate e o acesso é negado.
//
// Fica no projeto Api (e não no Negocio) porque é um detalhe de COMO a Api autentica.
// A regra de QUEM pode entrar continua no AutenticacaoNegocio, usado também pela Web.
public class GeradorDeToken
{
    private readonly ConfiguracaoJwt _config;

    public GeradorDeToken(ConfiguracaoJwt config) => _config = config;

    public TokenDto Gerar(UsuarioLogadoDto usuario)
    {
        var expira = DateTime.UtcNow.AddHours(_config.ExpiracaoHoras);

        // Nomes curtos e padronizados das claims; o Program.cs usa os mesmos nomes na validação.
        var claims = new[]
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("name", usuario.Nome),
            new Claim("email", usuario.Email),
            new Claim("role", usuario.Perfil)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.Emissor,
            audience: _config.Publico,
            claims: claims,
            expires: expira,
            signingCredentials: credenciais);

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expira,
            Usuario = usuario
        };
    }
}
