using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

// Única classe que conversa com a tabela Cliente (somente Entity Framework Core).
public class ClienteRepositorio : IClienteRepositorio
{
    private readonly AppDbContext _contexto;

    // INJEÇÃO DE DEPENDÊNCIA: o contexto chega pronto, com a connection string do appsettings.json.
    public ClienteRepositorio(AppDbContext contexto) => _contexto = contexto;

    // AsNoTracking: só leitura, mais rápido (o EF não "vigia" o objeto).
    public List<Cliente> Listar() =>
        _contexto.Clientes.AsNoTracking().OrderBy(c => c.Nome).ToList();

    public Cliente? ObterPorId(int id) =>
        _contexto.Clientes.FirstOrDefault(c => c.IdCliente == id);

    public Cliente? ObterPorEmail(string email) =>
        _contexto.Clientes.FirstOrDefault(c => c.Email == email);

    public bool EmailEmUso(string email) =>
        _contexto.Clientes.Any(c => c.Email == email);

    public int ContarAtivos() => _contexto.Clientes.Count(c => c.Ativo);

    public void Adicionar(Cliente cliente)
    {
        _contexto.Clientes.Add(cliente);
        try
        {
            _contexto.SaveChanges();
        }
        catch (DbUpdateException ex) when (ErrosSql.EhViolacaoDeUnicidade(ex))
        {
            throw new ConflitoDeDadosException("Já existe um cadastro com este e-mail.", ex);
        }
    }

    public void Salvar() => _contexto.SaveChanges();
}
