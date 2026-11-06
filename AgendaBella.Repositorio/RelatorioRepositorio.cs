using System.Data;
using AgendaBella.Modelos.Relatorios;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

// Só Dapper: relatórios são consultas de leitura, resolvidas por stored procedures.
public class RelatorioRepositorio : IRelatorioRepositorio
{
    private readonly AppDbContext _contexto;
    public RelatorioRepositorio(AppDbContext contexto) => _contexto = contexto;

    public List<ResumoServico> ResumoPorServico(DateTime inicio, DateTime fim) =>
        _contexto.Database.GetDbConnection()
            .Query<ResumoServico>("sp_Relatorio_Servicos_Periodo",
                new { DataInicio = inicio.Date, DataFim = fim.Date },
                commandType: CommandType.StoredProcedure)
            .ToList();

    public List<DemandaSemanal> DemandaSemanal(int semanas) =>
        _contexto.Database.GetDbConnection()
            .Query<DemandaSemanal>("sp_Relatorio_Demanda_Semanal",
                new { Semanas = semanas },
                commandType: CommandType.StoredProcedure)
            .ToList();

    public List<DemandaDiaSemana> DemandaPorDiaSemana(DateTime inicio, DateTime fim) =>
        _contexto.Database.GetDbConnection()
            .Query<DemandaDiaSemana>("sp_Relatorio_Demanda_DiaSemana",
                new { DataInicio = inicio.Date, DataFim = fim.Date },
                commandType: CommandType.StoredProcedure)
            .ToList();
}
