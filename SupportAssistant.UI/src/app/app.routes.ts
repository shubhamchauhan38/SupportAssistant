
import { Routes } from '@angular/router';

import { LoginComponent } from './features/auth/login/login.component';
import { EmployeeDashboardComponent } from './features/dashboard/employee-dashboard/employee-dashboard.component';
import { EngineerDashboardComponent } from './features/dashboard/engineer-dashboard/engineer-dashboard.component';
import { CreateTicket } from './features/tickets/create-ticket/create-ticket.component';

import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role-guard';

export const routes: Routes = [

  {
    path: 'login',
    component: LoginComponent
  },

  {
    path: 'employee/dashboard',
    component: EmployeeDashboardComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Employee'] }
  },

  {
    path: 'engineer/dashboard',
    component: EngineerDashboardComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['SupportEngineer'] }
  },

  {
    path: 'employee/tickets/new',
    component: CreateTicket,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Employee'] }
  },

  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  {
    path: '**',
    redirectTo: 'login'
  },
];
