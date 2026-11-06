namespace AgendaBella.Web.Helpers;

// Guarda o caminho físico da pasta de uploads. É registrada no Program.cs e entregue
// aos controllers por injeção de dependência.
public class PastaUploads
{
    public string Caminho { get; }
    public PastaUploads(string caminho) => Caminho = caminho;
}
