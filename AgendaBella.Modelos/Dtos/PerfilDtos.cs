using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos.Dtos;

// Entrada e saída das preferências de unhas da cliente.
public class PreferenciaDto
{
    [Display(Name = "Formato da unha")]
    [StringLength(50, ErrorMessage = "Máximo de 50 caracteres.")]
    public string? FormatoUnha { get; set; }

    [Display(Name = "Cor favorita")]
    [StringLength(50, ErrorMessage = "Máximo de 50 caracteres.")]
    public string? CorFavorita { get; set; }

    [Display(Name = "Estilo")]
    [StringLength(100, ErrorMessage = "Máximo de 100 caracteres.")]
    public string? Estilo { get; set; }

    [Display(Name = "Observações (alergias, cuidados...)")]
    [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
    public string? Observacao { get; set; }

    public static PreferenciaDto De(PreferenciaCliente? p) => p is null ? new PreferenciaDto() : new PreferenciaDto
    {
        FormatoUnha = p.FormatoUnha, CorFavorita = p.CorFavorita, Estilo = p.Estilo, Observacao = p.Observacao
    };
}

public class MovimentacaoPontosDto
{
    public int Quantidade { get; set; }          // já com sinal (+ crédito, - débito)
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime Data { get; set; }

    public static MovimentacaoPontosDto De(MovimentacaoPontos m) => new()
    {
        Quantidade = m.ValorComSinal, Tipo = m.Tipo, Descricao = m.Descricao, Data = m.DataMovimentacao
    };
}

public class SaldoPontosDto
{
    public int Saldo { get; set; }
    public List<MovimentacaoPontosDto> Extrato { get; set; } = new();
}

public class ResgatarPontosDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Cliente inválida.")]
    public int IdCliente { get; set; }

    [Range(1, 100000, ErrorMessage = "Informe uma quantidade de pontos maior que zero.")]
    public int Pontos { get; set; }

    [StringLength(255)]
    public string? Descricao { get; set; }
}
