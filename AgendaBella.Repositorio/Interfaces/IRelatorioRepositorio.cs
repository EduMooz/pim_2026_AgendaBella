using AgendaBella.Modelos.Relatorios;

namespace AgendaBella.Repositorio.Interfaces;

// Todas as consultas deste repositório são STORED PROCEDURES executadas com Dapper.
public interface IRelatorioRepositorio
{
    List<ResumoServico> ResumoPorServico(DateTime inicio, DateTime fim);
    List<DemandaSemanal> DemandaSemanal(int semanas);
    List<DemandaDiaSemana> DemandaPorDiaSemana(DateTime inicio, DateTime fim);
}
