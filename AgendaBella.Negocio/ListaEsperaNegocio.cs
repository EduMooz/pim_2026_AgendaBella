using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Lista de espera: a cliente entra quando o dia que ela quer está lotado.
// Quando uma vaga abre, a TRIGGER TR_Agendamento_ListaEspera muda o status para "Notificado".
public class ListaEsperaNegocio
{
    private readonly IListaEsperaRepositorio _lista;
    private readonly IServicoRepositorio _servicos;
    private readonly AgendamentoNegocio _agendamentos;
    private readonly IProfissionalRepositorio _profissionais;

    public ListaEsperaNegocio(
        IListaEsperaRepositorio lista,
        IServicoRepositorio servicos,
        AgendamentoNegocio agendamentos,
        IProfissionalRepositorio profissionais)
    {
        _lista = lista;
        _servicos = servicos;
        _agendamentos = agendamentos;
        _profissionais = profissionais;
    }

    public List<ListaEspera> ListarDoCliente(int idCliente) => _lista.ListarDoCliente(idCliente);
    public List<ListaEspera> ListarAtivas() => _lista.ListarAtivas();

    public ListaEspera Entrar(int idCliente, ListaEsperaCriarDto dto)
    {
        var servico = _servicos.ObterPorId(dto.IdServico);
        if (servico is null || !servico.Ativo)
            throw new RegraDeNegocioException("Serviço indisponível.");

        var data = dto.DataDesejada.Date;
        if (data < DateTime.Today)
            throw new RegraDeNegocioException("Escolha uma data futura.");

        TimeSpan? horario = null;
        if (!string.IsNullOrWhiteSpace(dto.HorarioDesejado))
        {
            if (!RegrasAgendamento.TentarLerHorario(dto.HorarioDesejado, out var h))
                throw new RegraDeNegocioException("Horário inválido.");
            horario = h;
        }

        // Regra: só entra na fila quem NÃO consegue agendar. Se existe horário livre, deve agendar.
        var livres = _agendamentos.ListarHorariosDisponiveis(data, servico.IdServico);
        var temVaga = horario is null ? livres.Count > 0 : livres.Any(x => x.Horario == horario);
        if (temVaga)
            throw new RegraDeNegocioException("Há horário disponível para esta escolha. Faça o agendamento direto.");

        if (_lista.JaExisteAtiva(idCliente, servico.IdServico, data))
            throw new RegraDeNegocioException("Você já está na lista de espera deste serviço neste dia.");

        var item = new ListaEspera
        {
            IdCliente = idCliente,
            IdProfissional = _profissionais.ObterPadrao()?.IdProfissional,
            IdServico = servico.IdServico,
            DataDesejada = data,
            HorarioDesejado = horario
        };

        try
        {
            _lista.Adicionar(item);
        }
        catch (ConflitoDeDadosException ex)
        {
            throw new RegraDeNegocioException(ex.Message);
        }
        return item;
    }

    public void Sair(int id, int idCliente)
    {
        var item = _lista.ObterPorId(id);
        if (item is null || item.IdCliente != idCliente)
            throw new RegraDeNegocioException("Solicitação não encontrada.");
        if (item.Status is StatusListaEspera.Atendido or StatusListaEspera.Cancelado)
            throw new RegraDeNegocioException("Esta solicitação já foi encerrada.");

        item.Status = StatusListaEspera.Cancelado;
        _lista.Salvar();
    }
}
