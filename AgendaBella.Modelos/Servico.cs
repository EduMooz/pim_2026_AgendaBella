using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

public class Servico
{
    public int IdServico { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descricao { get; set; }

    [Range(0, 99999.99)]
    public decimal Preco { get; set; }

    // Duração em minutos.
    [Range(1, 600)]
    public int Duracao { get; set; }

    // Em vez de apagar, o sistema desativa (agendamentos antigos continuam válidos).
    public bool Ativo { get; set; } = true;
}
