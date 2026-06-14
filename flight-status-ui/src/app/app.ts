import {
  Component,
  inject,
  signal,
  effect,
  ElementRef,
  viewChild,
  AfterViewInit,
  OnDestroy,
} from '@angular/core';
import { DatePipe, NgClass, NgIf } from '@angular/common';
import { finalize } from 'rxjs';
import {
  FlightStatusService,
  FlightStatusResult,
} from './flight-status.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgIf, NgClass, DatePipe],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App implements AfterViewInit, OnDestroy {
  private readonly flightStatusService = inject(FlightStatusService);

  readonly flightNumber = signal('AI101');
  readonly flightDate = signal(new Date().toISOString().slice(0, 10));
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly statusResult = signal<FlightStatusResult | null>(null);
  readonly animating = signal(false);
  readonly landed = signal(false);
  readonly currentTime = signal(this.formatClock(new Date()));

  readonly animStage = viewChild<ElementRef<HTMLDivElement>>('animStage');

  private animFrame: number | null = null;
  private clockInterval: ReturnType<typeof setInterval> | null = null;

  constructor() {
    effect(() => {
      const result = this.statusResult();
      if (result) {
        // Defer so: (1) *ngIf renders #animStage into DOM first,
        // (2) signal writes happen outside effect context
        setTimeout(() => {
          if (!this.animating()) {
            this.playLandingAnimation();
          }
        }, 50);
      }
    });
  }

  ngAfterViewInit(): void {
    this.clockInterval = setInterval(() => {
      this.currentTime.set(this.formatClock(new Date()));
    }, 1000);
  }

  private formatClock(d: Date): string {
    return d.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
    });
  }

  statusLabel(status: string | number): string {
    const map: Record<number, string> = {
      0: 'UNKNOWN',
      1: 'ON TIME',
      2: 'DELAYED',
      3: 'CANCELLED',
      4: 'DIVERTED',
    };
    if (typeof status === 'number') return map[status] ?? 'UNKNOWN';
    return status?.toUpperCase() ?? 'UNKNOWN';
  }

  statusClass(status: string | number): string {
    const map: Record<number, string> = {
      0: 'unknown',
      1: 'ontime',
      2: 'delayed',
      3: 'cancelled',
      4: 'diverted',
    };
    const normalized =
      typeof status === 'number'
        ? map[status]
        : status?.toLowerCase().replace(/\s/g, '');

    switch (normalized) {
      case 'ontime':
        return 'badge-green';
      case 'delayed':
        return 'badge-amber';
      case 'cancelled':
      case 'diverted':
        return 'badge-red';
      default:
        return 'badge-grey';
    }
  }

  searchStatus(): void {
    const flightNumber = this.flightNumber().trim();
    const date = this.flightDate();

    if (!flightNumber || !date) {
      this.error.set('ENTER FLIGHT NUMBER AND DATE');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.statusResult.set(null);
    this.landed.set(false);
    this.animating.set(false);

    this.flightStatusService
      .searchStatus(flightNumber, date)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.statusResult.set(result),
        error: (err) =>
          this.error.set(
            err?.message?.toUpperCase() ?? 'UNABLE TO LOAD FLIGHT STATUS'
          ),
      });
  }

  private playLandingAnimation(): void {
    this.animating.set(true);
    this.landed.set(false);

    const el = this.animStage()?.nativeElement;
    if (!el) {
      this.animating.set(false);
      this.landed.set(true);
      return;
    }

    const svg = el.querySelector('#landingSvg') as SVGSVGElement | null;
    const path = el.querySelector('#flightPath') as SVGPathElement | null;
    const planeGroup = el.querySelector('#planeG') as SVGGElement | null;
    const trailPath = el.querySelector('#trailLine') as SVGPathElement | null;

    if (!svg || !path || !planeGroup || !trailPath) {
      this.animating.set(false);
      this.landed.set(true);
      return;
    }

    const totalLen = path.getTotalLength();
    const duration = 2800;
    let startTime: number | null = null;

    const ease = (t: number) =>
      t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

    const step = (ts: number) => {
      if (!startTime) startTime = ts;
      const rawT = Math.min((ts - startTime) / duration, 1);
      const t = ease(rawT);
      const d = t * totalLen;
      const p = path.getPointAtLength(d);
      const p2 = path.getPointAtLength(Math.min(d + 2, totalLen));
      const angle =
        (Math.atan2(p2.y - p.y, p2.x - p.x) * 180) / Math.PI;
      const scale = (0.6 + t * 0.6).toFixed(2);

      planeGroup.setAttribute(
        'transform',
        `translate(${p.x},${p.y}) rotate(${angle}) scale(${scale})`
      );
      trailPath.setAttribute('stroke-dasharray', `${d} ${totalLen}`);

      if (rawT < 1) {
        this.animFrame = requestAnimationFrame(step);
      } else {
        this.animating.set(false);
        this.landed.set(true);
      }
    };

    this.animFrame = requestAnimationFrame(step);
  }

  ngOnDestroy(): void {
    if (this.animFrame) cancelAnimationFrame(this.animFrame);
    if (this.clockInterval) clearInterval(this.clockInterval);
  }
}