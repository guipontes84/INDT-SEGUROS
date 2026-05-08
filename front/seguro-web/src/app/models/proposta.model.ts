export type PropostaStatus = 'EmAnalise' | 'Aprovada' | 'Rejeitada' | 'Cancelada';
export type TipoSeguro =
  | 'Auto'
  | 'Residencial'
  | 'Vida'
  | 'Empresarial'
  | 'Viagem'
  | 'Saude'
  | 'Odontologico'
  | 'Moto'
  | 'Celular'
  | 'Equipamentos'
  | 'Patrimonial'
  | 'Condominio'
  | 'Previdencia'
  | 'Rural'
  | 'Nautico'
  | 'Transporte'
  | 'ResponsabilidadeCivil'
  | 'Pet'
  | 'AcidentesPessoais'
  | 'Garantia';

export interface TipoSeguroOpcao {
  id: TipoSeguro;
  nome: string;
}

export interface Contratacao {
  id: string;
  propostaId: string;
  dataContratacao: string;
}

export interface Proposta {
  id: string;
  nomeCliente: string;
  documentoCliente: string;
  tipoSeguro: TipoSeguro;
  valorSeguro: number;
  status: PropostaStatus;
  dataCriacao: string;
  dataAtualizacao: string;
}

export interface PropostaApiResponse extends Omit<Proposta, 'status'> {
  status: PropostaStatus | number;
}
