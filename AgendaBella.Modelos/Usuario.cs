using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

// CLASSE BASE ABSTRATA (HERANÇA): dados que Cliente e Profissional têm em comum.
// "abstract" = não dá para criar um Usuario direto; só Cliente ou Profissional.
// O EF Core não mapeia esta classe como tabela: ele "copia" as propriedades
// para as tabelas Cliente e Profissional.
public abstract class Usuario
{
    // ENCAPSULAMENTO: o campo é privado; só o setter da propriedade mexe nele,
    // e o setter padroniza o e-mail (sem espaços, tudo minúsculo).
    private string _email = string.Empty;

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(150), EmailAddress]
    public string Email
    {
        get => _email;
        set => _email = (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    // Guarda o HASH da senha (PBKDF2), nunca a senha digitada.
    [Required, StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    // POLIMORFISMO: cada classe filha responde de um jeito.
    public abstract string TipoUsuario();

    // Método virtual: a filha PODE sobrescrever, mas não é obrigada.
    public virtual string Saudacao() => $"Olá, {Nome}!";
}
