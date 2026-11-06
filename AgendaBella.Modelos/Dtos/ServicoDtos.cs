using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos.Dtos;

// SAÍDA
public class ServicoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int Duracao { get; set; }
    public bool Ativo { get; set; }

    public static ServicoDto De(Servico s) => new()
    {
        Id = s.IdServico, Nome = s.Nome, Descricao = s.Descricao,
        Preco = s.Preco, Duracao = s.Duracao, Ativo = s.Ativo
    };
}

// ENTRADA (cadastro e edição)
public class ServicoSalvarDto
{
    [Display(Name = "Nome do serviço")]
    [Required(ErrorMessage = "Informe o nome do serviço.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    [StringLength(255, ErrorMessage = "A descrição pode ter no máximo 255 caracteres.")]
    public string? Descricao { get; set; }

    [Display(Name = "Preço (R$)")]
    [Range(0, 99999.99, ErrorMessage = "Informe um preço entre 0 e 99.999,99.")]
    public decimal Preco { get; set; }

    [Display(Name = "Duração (minutos)")]
    [Range(5, 600, ErrorMessage = "A duração deve ter entre 5 e 600 minutos.")]
    public int Duracao { get; set; } = 30;

    [Display(Name = "Ativo")]
    public bool Ativo { get; set; } = true;

    public Servico ParaServico(int id = 0) => new()
    {
        IdServico = id, Nome = Nome, Descricao = Descricao,
        Preco = Preco, Duracao = Duracao, Ativo = Ativo
    };

    public static ServicoSalvarDto De(Servico s) => new()
    {
        Nome = s.Nome, Descricao = s.Descricao, Preco = s.Preco, Duracao = s.Duracao, Ativo = s.Ativo
    };
}
