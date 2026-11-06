using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

// Contrato do repositório. A camada Negocio conhece só esta interface, nunca a classe concreta.
public interface IClienteRepositorio
{
    List<Cliente> Listar();
    Cliente? ObterPorId(int id);
    Cliente? ObterPorEmail(string email);
    bool EmailEmUso(string email);
    int ContarAtivos();
    void Adicionar(Cliente cliente);
    void Salvar();   // grava alterações de entidades já carregadas (rastreadas)
}
