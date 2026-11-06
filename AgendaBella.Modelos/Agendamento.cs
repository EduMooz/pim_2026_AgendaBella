using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

// Entidade central. Mostra ENCAPSULAMENTO de verdade: o Status tem "private set",
// então ninguém de fora escreve agendamento.Status = "qualquer coisa".
// A única forma de mudar é pelos métodos abaixo, que seguem as regras de transição:
//
//   Pendente --Confirmar--> Confirmado --Concluir--> Concluido
//      \                        \
//       `----Cancelar-----------`--> Cancelado
//
// Cada método devolve true (mudou) ou false (transição não permitida).
// A camada Negocio transforma o false em uma mensagem para o usuário.
public class Agendamento
{
    public int IdAgendamento { get; set; }
    public int IdCliente { get; set; }
    public int IdProfissional { get; set; }
    public int IdServico { get; set; }

    public DateTime DataAgendamento { get; set; }
    public TimeSpan Horario { get; set; }

    public string Status { get; private set; } = StatusAgendamento.Pendente;

    [StringLength(500)]
    public string? ImagemInspiracao { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }

    public DateTime DataCriacao { get; set; }

    public Cliente? Cliente { get; set; }
    public Profissional? Profissional { get; set; }
    public Servico? Servico { get; set; }
    public ICollection<HistoricoAgendamento> Historico { get; set; } = new List<HistoricoAgendamento>();

    // Data + hora juntas (só leitura, não vira coluna).
    public DateTime Inicio => DataAgendamento.Date + Horario;

    public bool EstaAtivo => Status is StatusAgendamento.Pendente or StatusAgendamento.Confirmado;

    public bool Confirmar()
    {
        if (Status != StatusAgendamento.Pendente) return false;
        Status = StatusAgendamento.Confirmado;
        return true;
    }

    public bool Concluir()
    {
        if (Status != StatusAgendamento.Confirmado) return false;
        Status = StatusAgendamento.Concluido;
        return true;
    }

    public bool Cancelar()
    {
        if (!EstaAtivo) return false;
        Status = StatusAgendamento.Cancelado;
        return true;
    }

    // Reagendar só vale para agendamento ativo.
    public bool Reagendar(DateTime novaData, TimeSpan novoHorario)
    {
        if (!EstaAtivo) return false;
        DataAgendamento = novaData.Date;
        Horario = novoHorario;
        return true;
    }
}
