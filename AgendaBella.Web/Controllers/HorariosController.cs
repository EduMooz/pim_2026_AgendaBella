using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// Expediente semanal e bloqueios de agenda (folgas, feriados...).
[Authorize(Roles = Perfis.Profissional)]
public class HorariosController : BaseController
{
    private readonly DisponibilidadeNegocio _negocio;
    public HorariosController(DisponibilidadeNegocio negocio) => _negocio = negocio;

    public IActionResult Index() => View(Montar());

    private HorariosAdminViewModel Montar() => new()
    {
        Horarios = _negocio.ListarHorarios().Select(HorarioTrabalhoDto.De).ToList(),
        Bloqueios = _negocio.ListarBloqueios().Select(BloqueioDto.De).ToList(),
        NovoBloqueio = new BloqueioSalvarDto { Data = DateTime.Today }
    };

    [HttpPost]
    public IActionResult AdicionarHorario([Bind(Prefix = "NovoHorario")] HorarioTrabalhoSalvarDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelo = Montar();
            modelo.NovoHorario = dto;
            return View("Index", modelo);
        }
        return Tentar(() => _negocio.AdicionarHorario(dto), "Horário de trabalho adicionado.", nameof(Index));
    }

    [HttpPost]
    public IActionResult RemoverHorario(int id) =>
        Tentar(() => _negocio.RemoverHorario(id), "Horário removido.", nameof(Index));

    [HttpPost]
    public IActionResult AdicionarBloqueio([Bind(Prefix = "NovoBloqueio")] BloqueioSalvarDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelo = Montar();
            modelo.NovoBloqueio = dto;
            return View("Index", modelo);
        }
        return Tentar(() => _negocio.AdicionarBloqueio(dto), "Bloqueio adicionado.", nameof(Index));
    }

    [HttpPost]
    public IActionResult RemoverBloqueio(int id) =>
        Tentar(() => _negocio.RemoverBloqueio(id), "Bloqueio removido.", nameof(Index));
}
