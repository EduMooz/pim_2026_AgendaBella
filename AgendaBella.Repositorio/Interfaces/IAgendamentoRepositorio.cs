using AgendaBella.Modelos;
using AgendaBella.Modelos.Relatorios;

namespace AgendaBella.Repositorio.Interfaces;

public interface IAgendamentoRepositorio
{
    Agendamento? ObterPorId(int id);                       // rastreado (permite alterar e Salvar)
    List<Agendamento> ListarDoCliente(int idCliente);
    List<Agendamento> ListarDoDia(DateTime data);
    List<Agendamento> ListarProximos(int quantidade);
    int ContarDoDia(DateTime data);
    int ContarPendentes();
    bool ClienteTemAgendamentoNoDia(int idCliente, DateTime data, int? ignorarId = null);
    List<HistoricoAgendamento> ListarHistorico(int idAgendamento);
    void Adicionar(Agendamento agendamento);               // pode lançar ConflitoDeDadosException
    void Salvar();                                         // idem

    // Executa a STORED PROCEDURE sp_Horarios_Disponiveis (Dapper).
    List<HorarioDisponivel> ListarHorariosDisponiveis(int idProfissional, DateTime data, int idServico, int? ignorarAgendamento = null);
}
