using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

public interface IServicoRepositorio
{
    List<Servico> Listar(bool somenteAtivos);
    Servico? ObterPorId(int id);
    bool PossuiAgendamentos(int idServico);
    void Adicionar(Servico servico);
    void Remover(Servico servico);
    void Salvar();
}
