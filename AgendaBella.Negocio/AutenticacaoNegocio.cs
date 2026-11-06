using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Login, cadastro de cliente e redefinição de senha.
// Web (cookie) e Api (JWT) usam esta MESMA classe: a regra de "quem pode entrar" é uma só.
public class AutenticacaoNegocio
{
    private readonly IClienteRepositorio _clientes;
    private readonly IProfissionalRepositorio _profissionais;

    public AutenticacaoNegocio(IClienteRepositorio clientes, IProfissionalRepositorio profissionais)
    {
        _clientes = clientes;
        _profissionais = profissionais;
    }

    // Devolve o usuário logado ou null quando e-mail/senha não conferem.
    // A mensagem de erro é sempre a mesma, para não revelar se o e-mail existe.
    public UsuarioLogadoDto? Autenticar(string email, string senha)
    {
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        senha ??= string.Empty;

        var cliente = _clientes.ObterPorEmail(email);
        if (cliente is { Ativo: true } && SenhaHasher.Verificar(senha, cliente.Senha))
            return new UsuarioLogadoDto { Id = cliente.IdCliente, Nome = cliente.Nome, Email = cliente.Email, Perfil = cliente.TipoUsuario() };

        var prof = _profissionais.ObterPorEmail(email);
        if (prof is not null && SenhaHasher.Verificar(senha, prof.Senha))
            return new UsuarioLogadoDto { Id = prof.IdProfissional, Nome = prof.Nome, Email = prof.Email, Perfil = prof.TipoUsuario() };

        return null;
    }

    public Cliente CadastrarCliente(CadastroClienteDto dto)
    {
        var cliente = dto.ParaCliente();
        cliente.Nome = cliente.Nome.Trim();
        cliente.Telefone = cliente.Telefone?.Trim();

        // O mesmo e-mail não pode existir como cliente NEM como profissional
        // (senão o login ficaria ambíguo).
        if (_clientes.EmailEmUso(cliente.Email) || _profissionais.ObterPorEmail(cliente.Email) is not null)
            throw new RegraDeNegocioException("Já existe um cadastro com este e-mail.");

        cliente.Senha = SenhaHasher.Gerar(dto.Senha);   // grava o hash, nunca a senha pura
        cliente.Ativo = true;

        try
        {
            _clientes.Adicionar(cliente);
        }
        catch (ConflitoDeDadosException ex)
        {
            throw new RegraDeNegocioException(ex.Message);
        }
        return cliente;
    }

    public void RedefinirSenha(RedefinirSenhaDto dto)
    {
        const string mensagem = "Não foi possível validar os dados informados.";

        var cliente = _clientes.ObterPorEmail(dto.Email.Trim().ToLowerInvariant());
        if (cliente is null || cliente.Telefone is null || SoDigitos(cliente.Telefone) != SoDigitos(dto.Telefone))
            throw new RegraDeNegocioException(mensagem);

        cliente.Senha = SenhaHasher.Gerar(dto.NovaSenha);
        _clientes.Salvar();
    }

    private static string SoDigitos(string texto) => new(texto.Where(char.IsDigit).ToArray());
}
