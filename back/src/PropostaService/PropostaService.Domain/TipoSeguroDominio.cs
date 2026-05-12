namespace PropostaService.Domain;

public sealed class TipoSeguroDominio
{
    private TipoSeguroDominio()
    {
        Chave = TipoSeguro.Auto;
        Nome = string.Empty;
    }

    public TipoSeguroDominio(Guid id, TipoSeguro chave, string nome)
    {
        Id = id;
        Chave = chave;
        Nome = nome;
    }

    public Guid Id { get; private set; }
    public TipoSeguro Chave { get; private set; }
    public string Nome { get; private set; }
}
