import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PlatformApi } from '../core/api.service';
import { PlatformUsageSummary } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink],
  styles: [`
    .kpis { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
    .kpi-card .label { color: var(--muted); font-size: 12px; text-transform: uppercase; }
    .bar { height: 8px; background: var(--panel-2); border-radius: 4px; overflow: hidden; }
    .bar > span { display: block; height: 100%; background: var(--accent); }
  `],
  template: `
    <h1>Dashboard</h1>
    <p class="muted">Platform-wide state at a glance.</p>
    @if (error()) { <div class="err">{{ error() }}</div> }
    @if (data(); as d) {
      <div class="kpis">
        <div class="card kpi-card"><div class="label">Tenants</div><div class="kpi">{{ d.tenantsTotal }}</div></div>
        <div class="card kpi-card"><div class="label">Active</div><div class="kpi">{{ d.tenantsActive }}</div></div>
        <div class="card kpi-card"><div class="label">Trial</div><div class="kpi">{{ d.tenantsTrial }}</div></div>
        <div class="card kpi-card"><div class="label">Est. MRR</div><div class="kpi">{{ d.estimatedMrr | number:'1.0-0' }} <span class="muted" style="font-size:13px">{{ d.currency }}</span></div></div>
      </div>

      <h2>Module adoption</h2>
      <div class="card grid">
        @for (m of d.moduleAdoption; track m.moduleKey) {
          <div>
            <div class="row" style="justify-content:space-between"><span>{{ m.moduleName }}</span><span class="muted">{{ m.tenantCount }} / {{ d.tenantsTotal }}</span></div>
            <div class="bar"><span [style.width.%]="d.tenantsTotal ? (m.tenantCount * 100 / d.tenantsTotal) : 0"></span></div>
          </div>
        }
      </div>

      <h2>Recent activity</h2>
      <div class="card">
        <table>
          <thead><tr><th>When</th><th>Admin</th><th>Action</th><th>Detail</th></tr></thead>
          <tbody>
            @for (a of d.recentActivity; track a.id) {
              <tr>
                <td class="muted">{{ a.createdOn | date:'short' }}</td>
                <td>{{ a.adminEmail }}</td>
                <td><span class="pill mute">{{ a.action }}</span></td>
                <td class="muted">
                  @if (a.tenantId) { <a [routerLink]="['/tenants', a.tenantId]">tenant</a> · }
                  {{ a.detail }}
                </td>
              </tr>
            } @empty { <tr><td colspan="4" class="muted">Nothing yet.</td></tr> }
          </tbody>
        </table>
      </div>
    } @else if (!error()) {
      <p class="muted">Loading…</p>
    }
  `,
})
export class DashboardComponent {
  private api = inject(PlatformApi);
  data = signal<PlatformUsageSummary | null>(null);
  error = signal('');

  constructor() {
    this.api.usage().then((d) => this.data.set(d)).catch((e) => this.error.set(e.message));
  }
}
