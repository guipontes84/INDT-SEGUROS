namespace PropostaService.Domain;

public sealed class TipoSeguroDominio
{
    private TipoSeguroDominio()
    {
        Nome = string.Empty;
    }

    public TipoSeguroDominio(TipoSeguro id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    public TipoSeguro Id { get; private set; }
    public string Nome { get; private set; }
}
