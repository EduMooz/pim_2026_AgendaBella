using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

public class DisponibilidadeRepositorio : IDisponibilidadeRepositorio
{
    private readonly AppDbContext _contexto;
    public DisponibilidadeRepositorio(AppDbContext contexto) => _contexto = contexto;

    // ---------- Expediente ----------
    public List<HorarioTrabalho> ListarHorariosTrabalho(int idProfissional) =>
        _contexto.HorariosTrabalho.AsNoTracking()
            .Where(h => h.IdProfissional == idProfissional)
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
            .ToList();

    public HorarioTrabalho? ObterHorarioTrabalho(int id) =>
        _contexto.HorariosTrabalho.FirstOrDefault(h => h.IdHorarioTrabalho == id);

    // Duas faixas se sobrepõem quando: inicioA < fimB  E  fimA > inicioB.
    public bool HaSobreposicaoHorarioTrabalho(int idProfissional, int diaSemana, TimeSpan inicio, TimeSpan fim) =>
        _contexto.HorariosTrabalho.Any(h =>
            h.IdProfissional == idProfissional &&
            h.DiaSemana == diaSemana &&
            h.HoraInicio < fim && h.HoraFim > inicio);

    public void AdicionarHorarioTrabalho(HorarioTrabalho horario)
    {
        _contexto.HorariosTrabalho.Add(horario);
        _contexto.SaveChanges();
    }

    public void RemoverHorarioTrabalho(HorarioTrabalho horario)
    {
        _contexto.HorariosTrabalho.Remove(horario);
        _contexto.SaveChanges();
    }

    // ---------- Bloqueios ----------
    public List<BloqueioHorario> ListarBloqueios(int idProfissional, DateTime aPartirDe)
    {
        var inicio = aPartirDe.Date;
        return _contexto.BloqueiosHorario.AsNoTracking()
            .Where(b => b.IdProfissional == idProfissional && b.DataBloqueio >= inicio)
            .OrderBy(b => b.DataBloqueio).ThenBy(b => b.HoraInicio)
            .ToList();
    }

    public BloqueioHorario? ObterBloqueio(int id) =>
        _contexto.BloqueiosHorario.FirstOrDefault(b => b.IdBloqueio == id);

    public void AdicionarBloqueio(BloqueioHorario bloqueio)
    {
        _contexto.BloqueiosHorario.Add(bloqueio);
        _contexto.SaveChanges();
    }

    public void RemoverBloqueio(BloqueioHorario bloqueio)
    {
        _contexto.BloqueiosHorario.Remove(bloqueio);
        _contexto.SaveChanges();
    }
}
