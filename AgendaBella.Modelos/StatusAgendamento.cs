namespace AgendaBella.Modelos;

// Valores permitidos para Agendamento.Status (o banco também confere, via CHECK).
public static class StatusAgendamento
{
    public const string Pendente = "Pendente";
    public const string Confirmado = "Confirmado";
    public const string Concluido = "Concluido";
    public const string Cancelado = "Cancelado";
}

// Valores de ListaEspera.Status.
public static class StatusListaEspera
{
    public const string Aguardando = "Aguardando";
    public const string Notificado = "Notificado";
    public const string Atendido = "Atendido";
    public const string Cancelado = "Cancelado";
}

public static class TipoPontos
{
    public const string Credito = "Credito";
    public const string Debito = "Debito";
}
