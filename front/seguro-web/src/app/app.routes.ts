import { Routes } from '@angular/router';
import { PropostaFormComponent } from './pages/proposta-form/proposta-form.component';
import { PropostasComponent } from './pages/propostas/propostas.component';

export const routes: Routes = [
  { path: '', component: PropostasComponent },
  { path: 'propostas', component: PropostasComponent },
  { path: 'propostas/nova', component: PropostaFormComponent },
  { path: '**', redirectTo: '' }
];
