using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Expediente semanal e bloqueios de agenda da profissional.
public class DisponibilidadeNegocio
{
    private readonly IDisponibilidadeRepositorio _disponibilidade;
    private readonly IProfissionalRepositorio _profissionais;
    private readonly IAgendamentoRepositorio _agendamentos;

    public DisponibilidadeNegocio(
        IDisponibilidadeRepositorio disponibilidade,
        IProfissionalRepositorio profissionais,
        IAgendamentoRepositorio agendamentos)
    {
        _disponibilidade = disponibilidade;
        _profissionais = profissionais;
        _agendamentos = agendamentos;
    }

    private int IdProfissional() =>
        (_profissionais.ObterPadrao() ?? throw new RegraDeNegocioException("Nenhuma profissional ativa cadastrada.")).IdProfissional;

    // ---------- Expediente ----------
    public List<HorarioTrabalho> ListarHorarios() => _disponibilidade.ListarHorariosTrabalho(IdProfissional());

    public void AdicionarHorario(HorarioTrabalhoSalvarDto dto)
    {
        if (dto.DiaSemana is < 1 or > 7)
            throw new RegraDeNegocioException("Dia da semana inválido.");
        if (!RegrasAgendamento.TentarLerHorario(dto.HoraInicio, out var inicio) ||
            !RegrasAgendamento.TentarLerHorario(dto.HoraFim, out var fim))
            throw new RegraDeNegocioException("Horário inválido.");
        if (inicio >= fim)
            throw new RegraDeNegocioException("O horário final deve ser maior que o inicial.");

        var idProf = IdProfissional();
        if (_disponibilidade.HaSobreposicaoHorarioTrabalho(idProf, dto.DiaSemana, inicio, fim))
            throw new RegraDeNegocioException("Esta faixa se sobrepõe a outra já cadastrada para o mesmo dia.");

        _disponibilidade.AdicionarHorarioTrabalho(new HorarioTrabalho
        {
            IdProfissional = idProf, DiaSemana = dto.DiaSemana, HoraInicio = inicio, HoraFim = fim
        });
    }

    public bool RemoverHorario(int id)
    {
        var h = _disponibilidade.ObterHorarioTrabalho(id);
        if (h is null) return false;
        _disponibilidade.RemoverHorarioTrabalho(h);
        return true;
    }

    // ---------- Bloqueios ----------
    public List<BloqueioHorario> ListarBloqueios() =>
        _disponibilidade.ListarBloqueios(IdProfissional(), DateTime.Today);

    public void AdicionarBloqueio(BloqueioSalvarDto dto)
    {
        var data = dto.Data.Date;
        if (data < DateTime.Today)
            throw new RegraDeNegocioException("A data do bloqueio não pode estar no passado.");

        var temInicio = !string.IsNullOrWhiteSpace(dto.HoraInicio);
        var temFim = !string.IsNullOrWhiteSpace(dto.HoraFim);
        if (temInicio != temFim)
            throw new RegraDeNegocioException("Informe o início e o fim, ou deixe os dois vazios para bloquear o dia inteiro.");

        TimeSpan? inicio = null, fim = null;
        if (temInicio)
        {
            if (!RegrasAgendamento.TentarLerHorario(dto.HoraInicio, out var i) ||
                !RegrasAgendamento.TentarLerHorario(dto.HoraFim, out var f))
                throw new RegraDeNegocioException("Horário inválido.");
            if (i >= f) throw new RegraDeNegocioException("O horário final deve ser maior que o inicial.");
            inicio = i; fim = f;
        }

        // Não deixa bloquear um período em que já existem agendamentos ativos.
        var inicioBloqueio = inicio ?? TimeSpan.Zero;
        var fimBloqueio = fim ?? TimeSpan.FromHours(24);
        var conflito = _agendamentos.ListarDoDia(data).Any(a =>
            a.EstaAtivo &&
            a.Horario < fimBloqueio &&
            a.Horario + TimeSpan.FromMinutes(a.Servico?.Duracao ?? 0) > inicioBloqueio);
        if (conflito)
            throw new RegraDeNegocioException("Já existem agendamentos neste período. Cancele ou reagende antes de bloquear.");

        _disponibilidade.AdicionarBloqueio(new BloqueioHorario
        {
            IdProfissional = IdProfissional(),
            DataBloqueio = data,
            HoraInicio = inicio,
            HoraFim = fim,
            Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim()
        });
    }

    public bool RemoverBloqueio(int id)
    {
        var b = _disponibilidade.ObterBloqueio(id);
        if (b is null) return false;
        _disponibilidade.RemoverBloqueio(b);
        return true;
    }
}
