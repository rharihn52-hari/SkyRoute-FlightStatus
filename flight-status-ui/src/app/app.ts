import { Component, inject, signal } from '@angular/core';
import { DatePipe, JsonPipe, NgClass, NgIf } from '@angular/common';
import { finalize } from 'rxjs';
import { FlightStatusService, FlightStatusResult } from './flight-status.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgIf, NgClass, DatePipe, JsonPipe],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  private readonly flightStatusService = inject(FlightStatusService);

  readonly flightNumber = signal('AI101');
  readonly flightDate = signal(new Date().toISOString().slice(0, 10));
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly statusResult = signal<FlightStatusResult | null>(null);

 statusClass(status: any): string {

  const statusMap: Record<number, string> = {
    0: 'unknown',
    1: 'ontime',
    2: 'delayed',
    3: 'cancelled',
    4: 'diverted'
  };

  const normalized =
    typeof status === 'number'
      ? statusMap[status]
      : status?.toLowerCase();

  switch (normalized) {
    case 'ontime':
      return 'status-green';

    case 'delayed':
      return 'status-amber';

    case 'cancelled':
    case 'diverted':
      return 'status-red';

    default:
      return 'status-grey';
  }
}

  searchStatus(): void {
    const flightNumber = this.flightNumber().trim();
    const date = this.flightDate();

    if (!flightNumber || !date) {
      this.error.set('Please enter both a flight number and date.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.statusResult.set(null);

    this.flightStatusService
      .searchStatus(flightNumber, date)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.statusResult.set(result),
        error: (err) => this.error.set(err?.message ?? 'Unable to load flight status.'),
      });
  }
}
