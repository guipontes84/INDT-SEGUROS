import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Proposta, PropostaStatus, TipoSeguro } from '../../models/proposta.model';
import { ContratacaoService } from '../../services/contratacao.service';
import { PropostaService } from '../../services/proposta.service';

type StatusFiltro = PropostaStatus | '';

@Component({
  selector: 'app-propostas',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './propostas.component.html',
  styleUrl: './propostas.component.css'
})
export class PropostasComponent implements OnInit {
  propostas: Proposta[] = [];
  filtro: StatusFiltro = '';
  carregando = false;
  mensagem = '';
  erro = '';
  propostasContratadas = new Set<string>();

  constructor(
    private readonly propostaService: PropostaService,
    private readonly contratacaoService: ContratacaoService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.mensagem = '';
    this.erro = '';

    forkJoin({
      propostas: this.propostaService.listar(this.filtro || undefined),
      contratacoes: this.contratacaoService.listar()
    }).subscribe({
      next: ({ propostas, contratacoes }) => {
        this.propostas = propostas;
        this.propostasContratadas = new Set(contratacoes.map((contratacao) => contratacao.propostaId));
        this.carregando = false;
      },
      error: () => {
        this.erro = 'Nao foi possivel carregar as propostas.';
        this.carregando = false;
      }
    });
  }

  alterarStatus(proposta: Proposta, status: PropostaStatus): void {
    this.propostaService.alterarStatus(proposta.id, status).subscribe({
      next: () => this.carregar(),
      error: (error) => {
        this.erro = error?.error?.erro ?? 'Nao foi possivel alterar o status.';
      }
    });
  }

  contratar(proposta: Proposta): void {
    this.mensagem = '';
    this.erro = '';

    this.contratacaoService.contratar(proposta).subscribe({
      next: () => {
        this.mensagem = 'Proposta contratada com sucesso.';
        this.carregar();
      },
      error: (error) => {
        this.erro = error?.error?.erro ?? 'Nao foi possivel contratar a proposta.';
      }
    });
  }

  podeAnalisar(proposta: Proposta): boolean {
    return proposta.status === 'EmAnalise';
  }

  podeContratar(proposta: Proposta): boolean {
    return proposta.status === 'Aprovada' && !this.propostasContratadas.has(proposta.id);
  }

  podeCancelar(proposta: Proposta): boolean {
    return proposta.status === 'Aprovada' && !this.propostasContratadas.has(proposta.id);
  }

  estaContratada(proposta: Proposta): boolean {
    return this.propostasContratadas.has(proposta.id);
  }

  obterStatusTexto(status: PropostaStatus): string {
    const textos: Record<PropostaStatus, string> = {
      EmAnalise: 'Em Analise',
      Aprovada: 'Aprovada',
      Rejeitada: 'Rejeitada',
      Cancelada: 'Cancelada'
    };

    return textos[status];
  }

  obterTipoSeguroTexto(tipoSeguro: TipoSeguro): string {
    const textos: Record<TipoSeguro, string> = {
      Auto: 'Seguro Auto',
      Residencial: 'Seguro Residencial',
      Vida: 'Seguro Vida',
      Empresarial: 'Seguro Empresarial',
      Viagem: 'Seguro Viagem',
      Saude: 'Seguro Saúde',
      Odontologico: 'Seguro Odontológico',
      Moto: 'Seguro Moto',
      Celular: 'Seguro Celular',
      Equipamentos: 'Seguro Equipamentos',
      Patrimonial: 'Seguro Patrimonial',
      Condominio: 'Seguro Condomínio',
      Previdencia: 'Seguro Previdência',
      Rural: 'Seguro Rural',
      Nautico: 'Seguro Náutico',
      Transporte: 'Seguro Transporte',
      ResponsabilidadeCivil: 'Seguro Responsabilidade Civil',
      Pet: 'Seguro Pet',
      AcidentesPessoais: 'Seguro Acidentes Pessoais',
      Garantia: 'Seguro Garantia'
    };

    return textos[tipoSeguro] ?? tipoSeguro;
  }
}
