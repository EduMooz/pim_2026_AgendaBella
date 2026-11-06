namespace AgendaBella.Repositorio.Excecoes;

// Lançada pelo Repositorio quando o BANCO recusa a gravação por duplicidade
// (índice único). Exemplo: duas clientes tentam o mesmo horário ao mesmo tempo.
// A camada Negocio captura e transforma em uma mensagem amigável.
public class ConflitoDeDadosException : Exception
{
    public ConflitoDeDadosException(string mensagem, Exception? interna = null) : base(mensagem, interna) { }
}
