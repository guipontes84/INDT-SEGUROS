import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Contratacao, Proposta } from '../models/proposta.model';

@Injectable({ providedIn: 'root' })
export class ContratacaoService {
  private readonly apiUrl = 'http://localhost:5002/api/contratacoes';

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Contratacao[]> {
    return this.http.get<Contratacao[]>(this.apiUrl);
  }

  contratar(proposta: Proposta): Observable<Contratacao> {
    return this.http.post<Contratacao>(this.apiUrl, { propostaId: proposta.id });
  }
}
