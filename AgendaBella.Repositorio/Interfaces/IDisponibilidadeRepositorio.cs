using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

// Expediente semanal e bloqueios de agenda.
public interface IDisponibilidadeRepositorio
{
    List<HorarioTrabalho> ListarHorariosTrabalho(int idProfissional);
    HorarioTrabalho? ObterHorarioTrabalho(int id);
    bool HaSobreposicaoHorarioTrabalho(int idProfissional, int diaSemana, TimeSpan inicio, TimeSpan fim);
    void AdicionarHorarioTrabalho(HorarioTrabalho horario);
    void RemoverHorarioTrabalho(HorarioTrabalho horario);

    List<BloqueioHorario> ListarBloqueios(int idProfissional, DateTime aPartirDe);
    BloqueioHorario? ObterBloqueio(int id);
    void AdicionarBloqueio(BloqueioHorario bloqueio);
    void RemoverBloqueio(BloqueioHorario bloqueio);
}
