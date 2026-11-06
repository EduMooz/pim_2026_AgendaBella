using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

public interface IListaEsperaRepositorio
{
    List<ListaEspera> ListarDoCliente(int idCliente);
    List<ListaEspera> ListarAtivas();                      // Aguardando + Notificado (painel da profissional)
    ListaEspera? ObterPorId(int id);
    bool JaExisteAtiva(int idCliente, int idServico, DateTime data);
    int ContarAguardando();
    // Marca como Atendido as entradas Notificadas da cliente naquele dia.
    void MarcarAtendidas(int idCliente, DateTime data);
    void Adicionar(ListaEspera item);
    void Salvar();
}
