
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TicketService } from '../../../services/ticket';
import { Ticket } from '../../../models/ticket';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.css'
})
export class TicketList implements OnInit {

  tickets: Ticket[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private ticketService: TicketService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.ticketService.getTickets().subscribe({
      next: (response) => {
        this.tickets = response;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load tickets.';
        this.isLoading = false;
      }
    });
  }
}
