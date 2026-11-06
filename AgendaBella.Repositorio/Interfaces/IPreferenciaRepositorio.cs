using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

public interface IPreferenciaRepositorio
{
    PreferenciaCliente? ObterDoCliente(int idCliente);
    void Adicionar(PreferenciaCliente preferencia);
    void Salvar();
}
