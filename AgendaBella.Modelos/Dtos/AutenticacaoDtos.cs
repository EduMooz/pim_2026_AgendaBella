using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos.Dtos;

// ENTRADA do login (formulário da Web e POST /api/auth/login).
public class LoginDto
{
    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe a senha.")]
    public string Senha { get; set; } = string.Empty;
}

// ENTRADA do cadastro de cliente.
public class CadastroClienteDto
{
    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo 150 caracteres.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(72, MinimumLength = 6, ErrorMessage = "A senha deve ter de 6 a 72 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Telefone")]
    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
    public string Telefone { get; set; } = string.Empty;

    public Cliente ParaCliente() => new()
    {
        Nome = Nome,
        Email = Email,
        Senha = Senha,        // a camada Negocio troca pelo hash antes de gravar
        Telefone = Telefone
    };
}

// ENTRADA da redefinição de senha. Para provar que é a dona da conta,
// pede também o telefone cadastrado (limitação: sem envio de e-mail).
public class RedefinirSenhaDto
{
    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefone cadastrado")]
    [Required(ErrorMessage = "Informe o telefone cadastrado.")]
    public string Telefone { get; set; } = string.Empty;

    [Display(Name = "Nova senha")]
    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(72, MinimumLength = 6, ErrorMessage = "A senha deve ter de 6 a 72 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;

    [Display(Name = "Confirmar senha")]
    [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

// SAÍDA: quem está logado (nunca inclui a senha).
public class UsuarioLogadoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
}

// SAÍDA da Api no login: o token JWT que o app Flutter guarda e reenvia.
public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public UsuarioLogadoDto Usuario { get; set; } = new();
}
