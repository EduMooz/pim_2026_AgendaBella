using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

public enum ResultadoRemocao { NaoEncontrado, Removido, Desativado }

public class ServicoNegocio
{
    private readonly IServicoRepositorio _servicos;
    public ServicoNegocio(IServicoRepositorio servicos) => _servicos = servicos;

    public List<Servico> Listar(bool somenteAtivos = true) => _servicos.Listar(somenteAtivos);

    public Servico? ObterPorId(int id) => _servicos.ObterPorId(id);

    public Servico Cadastrar(ServicoSalvarDto dto)
    {
        var servico = dto.ParaServico();
        Normalizar(servico);
        _servicos.Adicionar(servico);
        return servico;
    }

    public void Atualizar(int id, ServicoSalvarDto dto)
    {
        var existente = _servicos.ObterPorId(id)
            ?? throw new RegraDeNegocioException("Serviço não encontrado.");

        existente.Nome = dto.Nome;
        existente.Descricao = dto.Descricao;
        existente.Preco = dto.Preco;
        existente.Duracao = dto.Duracao;
        existente.Ativo = dto.Ativo;
        Normalizar(existente);
        _servicos.Salvar();
    }

    // Se o serviço já foi usado em algum agendamento, o banco não deixa apagar
    // (chave estrangeira). Nesse caso o sistema DESATIVA, preservando o histórico.
    public ResultadoRemocao Remover(int id)
    {
        var servico = _servicos.ObterPorId(id);
        if (servico is null) return ResultadoRemocao.NaoEncontrado;

        if (_servicos.PossuiAgendamentos(id))
        {
            servico.Ativo = false;
            _servicos.Salvar();
            return ResultadoRemocao.Desativado;
        }

        _servicos.Remover(servico);
        return ResultadoRemocao.Removido;
    }

    private static void Normalizar(Servico s)
    {
        s.Nome = s.Nome.Trim();
        s.Descricao = string.IsNullOrWhiteSpace(s.Descricao) ? null : s.Descricao.Trim();
        if (s.Nome.Length == 0) throw new RegraDeNegocioException("Informe o nome do serviço.");
        if (s.Preco < 0) throw new RegraDeNegocioException("O preço não pode ser negativo.");
        if (s.Duracao <= 0) throw new RegraDeNegocioException("A duração precisa ser maior que zero.");
    }
}
