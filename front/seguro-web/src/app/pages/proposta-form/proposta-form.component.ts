import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TipoSeguroOpcao } from '../../models/proposta.model';
import { CriarPropostaRequest, PropostaService } from '../../services/proposta.service';

@Component({
  selector: 'app-proposta-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './proposta-form.component.html',
  styleUrl: './proposta-form.component.css'
})
export class PropostaFormComponent {
  proposta: CriarPropostaRequest = {
    nomeCliente: '',
    documentoCliente: '',
    tipoSeguro: '',
    valorSeguro: 0
  };

  salvando = false;
  carregandoTipos = false;
  erro = '';
  tiposSeguro: TipoSeguroOpcao[] = [];

  constructor(
    private readonly propostaService: PropostaService,
    private readonly router: Router
  ) {
    this.carregarTiposSeguro();
  }

  carregarTiposSeguro(): void {
    this.carregandoTipos = true;
    this.propostaService.listarTiposSeguro().subscribe({
      next: (tiposSeguro) => {
        this.tiposSeguro = tiposSeguro;
        this.carregandoTipos = false;
      },
      error: () => {
        this.erro = 'Nao foi possivel carregar os tipos de seguro.';
        this.carregandoTipos = false;
      }
    });
  }

  salvar(): void {
    this.erro = '';

    if (!this.proposta.nomeCliente || !this.proposta.documentoCliente || !this.proposta.tipoSeguro || this.proposta.valorSeguro <= 0) {
      this.erro = 'Preencha todos os campos obrigatorios.';
      return;
    }

    this.salvando = true;
    this.propostaService.criar(this.proposta).subscribe({
      next: () => this.router.navigateByUrl('/propostas'),
      error: (error) => {
        this.erro = error?.error?.erro ?? 'Nao foi possivel criar a proposta.';
        this.salvando = false;
      }
    });
  }
}
