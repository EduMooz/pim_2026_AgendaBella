using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

// Cliente É UM Usuario (herança) e acrescenta o que é só dele.
public class Cliente : Usuario
{
    public int IdCliente { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; }

    // Relacionamentos (1 cliente -> vários agendamentos, 1 preferência, vários movimentos de pontos)
    public PreferenciaCliente? Preferencia { get; set; }
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<MovimentacaoPontos> Pontos { get; set; } = new List<MovimentacaoPontos>();

    // POLIMORFISMO: sobrescreve o método abstrato da classe base.
    public override string TipoUsuario() => Perfis.Cliente;
}
