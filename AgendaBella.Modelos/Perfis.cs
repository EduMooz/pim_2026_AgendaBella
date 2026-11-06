namespace AgendaBella.Modelos;

// Nomes dos perfis de acesso. Usados na Web (cookie), na Api (JWT) e nos
// atributos [Authorize(Roles = ...)]. Ficam aqui para ninguém digitar o texto errado.
public static class Perfis
{
    public const string Cliente = "Cliente";
    public const string Profissional = "Profissional";
}
