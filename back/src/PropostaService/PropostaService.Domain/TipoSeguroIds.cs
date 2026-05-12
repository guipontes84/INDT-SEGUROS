using BaseComum;

namespace PropostaService.Domain;

public static class TipoSeguroIds
{
    public static Guid Obter(TipoSeguro tipoSeguro)
    {
        return tipoSeguro switch
        {
            TipoSeguro.Auto => Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TipoSeguro.Residencial => Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TipoSeguro.Vida => Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TipoSeguro.Empresarial => Guid.Parse("44444444-4444-4444-4444-444444444444"),
            TipoSeguro.Viagem => Guid.Parse("55555555-5555-5555-5555-555555555555"),
            TipoSeguro.Saude => Guid.Parse("66666666-6666-6666-6666-666666666666"),
            TipoSeguro.Odontologico => Guid.Parse("77777777-7777-7777-7777-777777777777"),
            TipoSeguro.Moto => Guid.Parse("88888888-8888-8888-8888-888888888888"),
            TipoSeguro.Celular => Guid.Parse("99999999-9999-9999-9999-999999999999"),
            TipoSeguro.Equipamentos => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            TipoSeguro.Patrimonial => Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            TipoSeguro.Condominio => Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            TipoSeguro.Previdencia => Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            TipoSeguro.Rural => Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            TipoSeguro.Nautico => Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            TipoSeguro.Transporte => Guid.Parse("12345678-1234-1234-1234-123456789001"),
            TipoSeguro.ResponsabilidadeCivil => Guid.Parse("12345678-1234-1234-1234-123456789002"),
            TipoSeguro.Pet => Guid.Parse("12345678-1234-1234-1234-123456789003"),
            TipoSeguro.AcidentesPessoais => Guid.Parse("12345678-1234-1234-1234-123456789004"),
            TipoSeguro.Garantia => Guid.Parse("12345678-1234-1234-1234-123456789005"),
            _ => throw new DomainException("Tipo de seguro invalido.")
        };
    }
}
