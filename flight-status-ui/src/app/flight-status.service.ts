import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, finalize, Observable, throwError } from 'rxjs';

export interface FlightStatusResult {
  flightNumber: string;
  flightDate: string;
  status: string | number;
  lastUpdatedUtc?: string;
  providerName: string;
  gate?: string;
  terminal?: string;
  delayReason?: string;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class FlightStatusService {
  private readonly http = inject(HttpClient);

  searchStatus(flightNumber: string, date: string): Observable<FlightStatusResult> {
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('date', date);

    return this.http.get<FlightStatusResult>('http://localhost:5000/flights/status', { params }).pipe(
      catchError((error: HttpErrorResponse) => {
        const message =
          error.error?.message ||
          error.error?.title ||
          error.message ||
          'Unable to load flight status.';
        return throwError(() => new Error(message));
      })
    );
  }
}
