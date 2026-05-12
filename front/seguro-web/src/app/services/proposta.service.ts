import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Proposta, PropostaApiResponse, PropostaStatus, TipoSeguro, TipoSeguroOpcao } from '../models/proposta.model';

export interface CriarPropostaRequest {
  nomeCliente: string;
  documentoCliente: string;
  tipoSeguro: TipoSeguro | '';
  valorSeguro: number;
}

@Injectable({ providedIn: 'root' })
export class PropostaService {
  private readonly apiUrl = 'http://localhost:5001/api/propostas';

  constructor(private readonly http: HttpClient) {}

  listar(status?: PropostaStatus): Observable<Proposta[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<PropostaApiResponse[]>(this.apiUrl, { params }).pipe(
      map((propostas) => propostas.map((proposta) => ({
        ...proposta,
        status: this.normalizarStatus(proposta.status)
      })))
    );
  }

  criar(request: CriarPropostaRequest): Observable<Proposta> {
    return this.http.post<PropostaApiResponse>(this.apiUrl, request).pipe(
      map((proposta) => ({
        ...proposta,
        status: this.normalizarStatus(proposta.status)
      }))
    );
  }

  listarTiposSeguro(): Observable<TipoSeguroOpcao[]> {
    return this.http.get<TipoSeguroOpcao[]>('http://localhost:5001/api/tipos-seguro');
  }

  alterarStatus(id: string, status: PropostaStatus): Observable<Proposta> {
    return this.http.patch<PropostaApiResponse>(`${this.apiUrl}/${id}/status`, { status }).pipe(
      map((proposta) => ({
        ...proposta,
        status: this.normalizarStatus(proposta.status)
      }))
    );
  }

  private normalizarStatus(status: PropostaStatus | number): PropostaStatus {
    switch (status) {
      case 1:
      case 'EmAnalise':
        return 'EmAnalise';
      case 2:
      case 'Aprovada':
        return 'Aprovada';
      case 3:
      case 'Rejeitada':
        return 'Rejeitada';
      case 4:
      case 'Cancelada':
        return 'Cancelada';
      case 5:
      case 'AguardandoContratacao':
        return 'AguardandoContratacao';
      case 6:
      case 'Contratado':
        return 'Contratado';
      default:
        return 'EmAnalise';
    }
  }
}
