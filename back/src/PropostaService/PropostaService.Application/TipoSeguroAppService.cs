using PropostaService.Domain;

namespace PropostaService.Application;

public sealed class TipoSeguroAppService
{
    public IReadOnlyCollection<TipoSeguroResponse> Listar()
    {
        return
        [
            new TipoSeguroResponse(TipoSeguro.Auto, "Seguro Auto"),
            new TipoSeguroResponse(TipoSeguro.Residencial, "Seguro Residencial"),
            new TipoSeguroResponse(TipoSeguro.Vida, "Seguro Vida"),
            new TipoSeguroResponse(TipoSeguro.Empresarial, "Seguro Empresarial"),
            new TipoSeguroResponse(TipoSeguro.Viagem, "Seguro Viagem"),
            new TipoSeguroResponse(TipoSeguro.Saude, "Seguro Saúde"),
            new TipoSeguroResponse(TipoSeguro.Odontologico, "Seguro Odontológico"),
            new TipoSeguroResponse(TipoSeguro.Moto, "Seguro Moto"),
            new TipoSeguroResponse(TipoSeguro.Celular, "Seguro Celular"),
            new TipoSeguroResponse(TipoSeguro.Equipamentos, "Seguro Equipamentos"),
            new TipoSeguroResponse(TipoSeguro.Patrimonial, "Seguro Patrimonial"),
            new TipoSeguroResponse(TipoSeguro.Condominio, "Seguro Condomínio"),
            new TipoSeguroResponse(TipoSeguro.Previdencia, "Seguro Previdência"),
            new TipoSeguroResponse(TipoSeguro.Rural, "Seguro Rural"),
            new TipoSeguroResponse(TipoSeguro.Nautico, "Seguro Náutico"),
            new TipoSeguroResponse(TipoSeguro.Transporte, "Seguro Transporte"),
            new TipoSeguroResponse(TipoSeguro.ResponsabilidadeCivil, "Seguro Responsabilidade Civil"),
            new TipoSeguroResponse(TipoSeguro.Pet, "Seguro Pet"),
            new TipoSeguroResponse(TipoSeguro.AcidentesPessoais, "Seguro Acidentes Pessoais"),
            new TipoSeguroResponse(TipoSeguro.Garantia, "Seguro Garantia")
        ];
    }
}
