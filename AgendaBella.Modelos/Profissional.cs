using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

public class Profissional : Usuario
{
    public int IdProfissional { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<HorarioTrabalho> HorariosTrabalho { get; set; } = new List<HorarioTrabalho>();

    public override string TipoUsuario() => Perfis.Profissional;

    // Sobrescreve o método virtual da base com uma saudação própria.
    public override string Saudacao() => $"Olá, {Nome}! Sua agenda está pronta.";
}
