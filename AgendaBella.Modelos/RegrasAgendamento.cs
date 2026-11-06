namespace AgendaBella.Modelos;

// Constantes das regras de negócio, compartilhadas por Negocio, telas e API.
public static class RegrasAgendamento
{
    // Cliente só cancela/reagenda com pelo menos esta antecedência.
    public const int AntecedenciaMinimaHoras = 2;

    // Pontos de fidelidade: 1 ponto para cada R$ 1,00 (aplicado pela trigger).
    public const int ReaisPorPonto = 1;

    public static bool TentarLerHorario(string? texto, out TimeSpan horario)
    {
        horario = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        return TimeSpan.TryParse(texto.Trim(), out horario) && horario >= TimeSpan.Zero && horario < TimeSpan.FromDays(1);
    }
}
