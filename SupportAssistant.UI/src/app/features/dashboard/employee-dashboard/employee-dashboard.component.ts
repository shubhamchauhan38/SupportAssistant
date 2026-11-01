import { Component } from '@angular/core';
import { TicketList } from '../../tickets/ticket-list/ticket-list.component';

@Component({
  selector: 'app-employee-dashboard',
  imports: [TicketList],
  templateUrl: './employee-dashboard.component.html',
  styleUrl: './employee-dashboard.component.css'
})
export class EmployeeDashboardComponent {

}
