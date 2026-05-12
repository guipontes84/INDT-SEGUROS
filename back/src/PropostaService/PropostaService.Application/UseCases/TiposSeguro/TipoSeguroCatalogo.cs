using PropostaService.Domain;

namespace PropostaService.Application.UseCases.TiposSeguro;

public static class TipoSeguroCatalogo
{
    public static readonly TipoSeguroResponse Auto = Criar(TipoSeguro.Auto, "Seguro Auto");
    public static readonly TipoSeguroResponse Residencial = Criar(TipoSeguro.Residencial, "Seguro Residencial");
    public static readonly TipoSeguroResponse Vida = Criar(TipoSeguro.Vida, "Seguro Vida");
    public static readonly TipoSeguroResponse Empresarial = Criar(TipoSeguro.Empresarial, "Seguro Empresarial");
    public static readonly TipoSeguroResponse Viagem = Criar(TipoSeguro.Viagem, "Seguro Viagem");
    public static readonly TipoSeguroResponse Saude = Criar(TipoSeguro.Saude, "Seguro Saude");
    public static readonly TipoSeguroResponse Odontologico = Criar(TipoSeguro.Odontologico, "Seguro Odontologico");
    public static readonly TipoSeguroResponse Moto = Criar(TipoSeguro.Moto, "Seguro Moto");
    public static readonly TipoSeguroResponse Celular = Criar(TipoSeguro.Celular, "Seguro Celular");
    public static readonly TipoSeguroResponse Equipamentos = Criar(TipoSeguro.Equipamentos, "Seguro Equipamentos");
    public static readonly TipoSeguroResponse Patrimonial = Criar(TipoSeguro.Patrimonial, "Seguro Patrimonial");
    public static readonly TipoSeguroResponse Condominio = Criar(TipoSeguro.Condominio, "Seguro Condominio");
    public static readonly TipoSeguroResponse Previdencia = Criar(TipoSeguro.Previdencia, "Seguro Previdencia");
    public static readonly TipoSeguroResponse Rural = Criar(TipoSeguro.Rural, "Seguro Rural");
    public static readonly TipoSeguroResponse Nautico = Criar(TipoSeguro.Nautico, "Seguro Nautico");
    public static readonly TipoSeguroResponse Transporte = Criar(TipoSeguro.Transporte, "Seguro Transporte");
    public static readonly TipoSeguroResponse ResponsabilidadeCivil = Criar(TipoSeguro.ResponsabilidadeCivil, "Seguro Responsabilidade Civil");
    public static readonly TipoSeguroResponse Pet = Criar(TipoSeguro.Pet, "Seguro Pet");
    public static readonly TipoSeguroResponse AcidentesPessoais = Criar(TipoSeguro.AcidentesPessoais, "Seguro Acidentes Pessoais");
    public static readonly TipoSeguroResponse Garantia = Criar(TipoSeguro.Garantia, "Seguro Garantia");

    public static IReadOnlyCollection<TipoSeguroResponse> Todos { get; } =
    [
        Auto,
        Residencial,
        Vida,
        Empresarial,
        Viagem,
        Saude,
        Odontologico,
        Moto,
        Celular,
        Equipamentos,
        Patrimonial,
        Condominio,
        Previdencia,
        Rural,
        Nautico,
        Transporte,
        ResponsabilidadeCivil,
        Pet,
        AcidentesPessoais,
        Garantia
    ];

    public static IReadOnlyCollection<TipoSeguroDominio> ParaDominio()
    {
        return Todos
            .Select(tipo => new TipoSeguroDominio(tipo.Uuid, tipo.Id, tipo.Nome))
            .ToArray();
    }

    private static TipoSeguroResponse Criar(TipoSeguro chave, string nome)
    {
        return new TipoSeguroResponse(TipoSeguroIds.Obter(chave), chave, chave.ToString(), nome);
    }
}
