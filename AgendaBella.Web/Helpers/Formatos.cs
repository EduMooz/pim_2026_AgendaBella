using System.Globalization;

namespace AgendaBella.Web.Helpers;

// Formatação para exibição (sempre no padrão brasileiro, independente da cultura do servidor).
public static class Formatos
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);              // R$ 45,00

    public static string Data(string isoData) =>                                         // 2026-10-10 -> 10/10/2026
        DateTime.TryParse(isoData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.ToString("dd/MM/yyyy", PtBr) : isoData;

    // "quarta-feira, 07 de outubro de 2026" -> "Quarta-feira, 07 de outubro de 2026"
    public static string DataLonga(DateTime data)
    {
        var texto = data.ToString("dddd, dd 'de' MMMM 'de' yyyy", PtBr);
        return char.ToUpper(texto[0], PtBr) + texto[1..];
    }

    // 14.3 -> "14,3" (vírgula decimal, padrão brasileiro)
    public static string Numero(double valor, int casas = 1) => valor.ToString("N" + casas, PtBr);

    public static string DataHora(DateTime data) => data.ToString("dd/MM/yyyy HH:mm", PtBr);

    public static string ClasseStatus(string status) => status switch
    {
        "Confirmado" => "confirmado",
        "Concluido" => "concluido",
        "Cancelado" => "cancelado",
        "Notificado" => "confirmado",
        "Atendido" => "concluido",
        _ => "pendente"
    };

    public static string TextoStatus(string status) => status == "Concluido" ? "Concluído" : status;
}
