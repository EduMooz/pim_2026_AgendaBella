namespace AgendaBella.Api.Seguranca;

// Caminho físico da pasta de uploads, registrado no Program.cs e injetado nos controllers.
public class PastaUploadsApi
{
    public string Caminho { get; }
    public PastaUploadsApi(string caminho) => Caminho = caminho;
}
