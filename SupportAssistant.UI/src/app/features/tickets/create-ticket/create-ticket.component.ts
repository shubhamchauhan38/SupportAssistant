
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TicketService } from '../../../services/ticket';

@Component({
  selector: 'app-create-ticket',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-ticket.component.html',
  styleUrl: './create-ticket.component.css'
})
export class CreateTicket {

  isSubmitting = false;
  errorMessage = '';

  ticketForm;

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
    private router: Router
  ) {
    this.ticketForm = this.fb.nonNullable.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      environment: ['Development', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.ticketService.createTicket(this.ticketForm.getRawValue()).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.router.navigate(['/employee/dashboard']);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error.error?.message || 'Unable to create ticket. Please try again.';
      }
    });
  }
}
