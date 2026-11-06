using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Modelos.Relatorios;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// O coração do sistema: criar, cancelar, reagendar, confirmar e concluir agendamentos.
// Nenhuma destas regras fica em controller ou view.
public class AgendamentoNegocio
{
    private readonly IAgendamentoRepositorio _agendamentos;
    private readonly IServicoRepositorio _servicos;
    private readonly IProfissionalRepositorio _profissionais;
    private readonly IListaEsperaRepositorio _listaEspera;

    public AgendamentoNegocio(
        IAgendamentoRepositorio agendamentos,
        IServicoRepositorio servicos,
        IProfissionalRepositorio profissionais,
        IListaEsperaRepositorio listaEspera)
    {
        _agendamentos = agendamentos;
        _servicos = servicos;
        _profissionais = profissionais;
        _listaEspera = listaEspera;
    }

    // ---------------- Consultas ----------------

    // Chama a STORED PROCEDURE (via repositório). Dia passado nunca tem horário.
    public List<HorarioDisponivel> ListarHorariosDisponiveis(DateTime data, int idServico, int? ignorarAgendamento = null)
    {
        if (data.Date < DateTime.Today) return new List<HorarioDisponivel>();
        var prof = ObterProfissionalPadrao();
        return _agendamentos.ListarHorariosDisponiveis(prof.IdProfissional, data, idServico, ignorarAgendamento);
    }

    public List<Agendamento> ListarDoCliente(int idCliente) => _agendamentos.ListarDoCliente(idCliente);
    public List<Agendamento> ListarDoDia(DateTime data) => _agendamentos.ListarDoDia(data);
    public List<Agendamento> ListarProximos(int quantidade) => _agendamentos.ListarProximos(quantidade);

    // Cliente só enxerga o que é dela; profissional enxerga tudo.
    public Agendamento? Obter(int id, int idUsuario, bool ehProfissional)
    {
        var ag = _agendamentos.ObterPorId(id);
        if (ag is null) return null;
        return ehProfissional || ag.IdCliente == idUsuario ? ag : null;
    }

    public List<HistoricoAgendamento> ListarHistorico(int id, int idUsuario, bool ehProfissional)
    {
        var ag = Obter(id, idUsuario, ehProfissional)
            ?? throw new RegraDeNegocioException("Agendamento não encontrado.");
        return _agendamentos.ListarHistorico(ag.IdAgendamento);
    }

    // ---------------- Criar ----------------

    public Agendamento Criar(int idCliente, AgendamentoCriarDto dto)
    {
        if (!RegrasAgendamento.TentarLerHorario(dto.Horario, out var horario))
            throw new RegraDeNegocioException("Horário inválido.");

        var servico = _servicos.ObterPorId(dto.IdServico);
        if (servico is null || !servico.Ativo)
            throw new RegraDeNegocioException("Serviço indisponível.");

        var data = dto.Data.Date;
        if (data + horario <= DateTime.Now)
            throw new RegraDeNegocioException("Escolha uma data e um horário futuros.");

        if (_agendamentos.ClienteTemAgendamentoNoDia(idCliente, data))
            throw new RegraDeNegocioException("Você já tem um agendamento neste dia. Reagende o existente ou escolha outra data.");

        var prof = ObterProfissionalPadrao();

        // A procedure é quem sabe o que está livre (expediente, bloqueios, outros agendamentos).
        var livres = _agendamentos.ListarHorariosDisponiveis(prof.IdProfissional, data, servico.IdServico);
        if (!livres.Any(h => h.Horario == horario))
            throw new RegraDeNegocioException("Este horário não está mais disponível. Escolha outro.");

        var agendamento = new Agendamento
        {
            IdCliente = idCliente,
            IdProfissional = prof.IdProfissional,
            IdServico = servico.IdServico,
            DataAgendamento = data,
            Horario = horario,
            Observacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim()
        };

        try
        {
            _agendamentos.Adicionar(agendamento);
        }
        catch (ConflitoDeDadosException ex)
        {
            // Duas clientes escolheram o mesmo horário no mesmo instante: o banco barrou a segunda.
            throw new RegraDeNegocioException(ex.Message);
        }

        // Se ela estava na lista de espera deste dia, a entrada passa a "Atendido".
        _listaEspera.MarcarAtendidas(idCliente, data);
        return agendamento;
    }

    public void AnexarImagem(int id, int idCliente, string caminhoRelativo)
    {
        var ag = ObterDaCliente(id, idCliente);
        if (!ag.EstaAtivo) throw new RegraDeNegocioException("Não é possível anexar imagem a um agendamento encerrado.");

        ag.ImagemInspiracao = caminhoRelativo;
        _agendamentos.Salvar();
    }

    // ---------------- Cancelar / reagendar (cliente ou profissional) ----------------

    public void Cancelar(int id, int idUsuario, bool ehProfissional)
    {
        var ag = ObterPermitido(id, idUsuario, ehProfissional);
        ExigirAntecedencia(ag, ehProfissional);

        if (!ag.Cancelar())
            throw new RegraDeNegocioException($"Um agendamento {ag.Status.ToLowerInvariant()} não pode ser cancelado.");
        _agendamentos.Salvar();   // as TRIGGERS registram o histórico e avisam a lista de espera
    }

    public void Reagendar(int id, int idUsuario, bool ehProfissional, ReagendarDto dto)
    {
        var ag = ObterPermitido(id, idUsuario, ehProfissional);
        ExigirAntecedencia(ag, ehProfissional);

        if (!RegrasAgendamento.TentarLerHorario(dto.Horario, out var horario))
            throw new RegraDeNegocioException("Horário inválido.");

        var data = dto.Data.Date;
        if (data + horario <= DateTime.Now)
            throw new RegraDeNegocioException("Escolha uma data e um horário futuros.");
        if (data == ag.DataAgendamento.Date && horario == ag.Horario)
            throw new RegraDeNegocioException("Escolha um dia ou horário diferente do atual.");
        if (_agendamentos.ClienteTemAgendamentoNoDia(ag.IdCliente, data, ignorarId: ag.IdAgendamento))
            throw new RegraDeNegocioException("A cliente já tem outro agendamento neste dia.");

        // ignorarAgendamento: o próprio agendamento não pode "bloquear" o novo horário.
        var livres = _agendamentos.ListarHorariosDisponiveis(ag.IdProfissional, data, ag.IdServico, ag.IdAgendamento);
        if (!livres.Any(h => h.Horario == horario))
            throw new RegraDeNegocioException("Este horário não está disponível. Escolha outro.");

        if (!ag.Reagendar(data, horario))
            throw new RegraDeNegocioException("Este agendamento não pode mais ser reagendado.");

        try
        {
            _agendamentos.Salvar();
        }
        catch (ConflitoDeDadosException ex)
        {
            throw new RegraDeNegocioException(ex.Message);
        }
    }

    // ---------------- Ações da profissional ----------------

    public void Confirmar(int id)
    {
        var ag = ObterOuFalhar(id);
        if (!ag.Confirmar())
            throw new RegraDeNegocioException("Só é possível confirmar um agendamento pendente.");
        _agendamentos.Salvar();
    }

    // Ao concluir, a trigger TR_Agendamento_Pontos credita os pontos de fidelidade.
    public void Concluir(int id)
    {
        var ag = ObterOuFalhar(id);
        if (ag.Inicio > DateTime.Now)
            throw new RegraDeNegocioException("O atendimento só pode ser concluído a partir do horário marcado.");
        if (!ag.Concluir())
            throw new RegraDeNegocioException("Só é possível concluir um agendamento confirmado.");
        _agendamentos.Salvar();
    }

    // ---------------- Apoio ----------------

    private Profissional ObterProfissionalPadrao() =>
        _profissionais.ObterPadrao() ?? throw new RegraDeNegocioException("Nenhuma profissional ativa cadastrada.");

    private Agendamento ObterOuFalhar(int id) =>
        _agendamentos.ObterPorId(id) ?? throw new RegraDeNegocioException("Agendamento não encontrado.");

    private Agendamento ObterDaCliente(int id, int idCliente)
    {
        var ag = ObterOuFalhar(id);
        // Mesma mensagem de "não encontrado": não revela que o agendamento existe para outra pessoa.
        if (ag.IdCliente != idCliente) throw new RegraDeNegocioException("Agendamento não encontrado.");
        return ag;
    }

    private Agendamento ObterPermitido(int id, int idUsuario, bool ehProfissional) =>
        ehProfissional ? ObterOuFalhar(id) : ObterDaCliente(id, idUsuario);

    // Só a cliente precisa respeitar a antecedência mínima; a profissional pode cancelar a qualquer hora.
    private static void ExigirAntecedencia(Agendamento ag, bool ehProfissional)
    {
        if (ehProfissional) return;
        if (ag.Inicio - DateTime.Now < TimeSpan.FromHours(RegrasAgendamento.AntecedenciaMinimaHoras))
            throw new RegraDeNegocioException(
                $"Alterações só podem ser feitas com {RegrasAgendamento.AntecedenciaMinimaHoras} horas de antecedência. Fale com a profissional.");
    }
}
