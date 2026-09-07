import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PlatformApi } from '../core/api.service';
import { AuditEntryDto } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <h1>Audit log</h1>
    <p class="muted">Every platform-admin action, newest first.</p>
    @if (error()) { <div class="err">{{ error() }}</div> }
    <div class="card">
      <table>
        <thead><tr><th>When</th><th>Admin</th><th>Action</th><th>Tenant</th><th>Detail</th></tr></thead>
        <tbody>
          @for (a of rows(); track a.id) {
            <tr>
              <td class="muted">{{ a.createdOn | date:'medium' }}</td>
              <td>{{ a.adminEmail }}</td>
              <td><span class="pill mute">{{ a.action }}</span></td>
              <td>@if (a.tenantId) { <a [routerLink]="['/tenants', a.tenantId]">open</a> } @else { — }</td>
              <td class="muted">{{ a.detail }}</td>
            </tr>
          } @empty { <tr><td colspan="5" class="muted">No activity.</td></tr> }
        </tbody>
      </table>
    </div>
  `,
})
export class AuditComponent {
  private api = inject(PlatformApi);
  rows = signal<AuditEntryDto[]>([]);
  error = signal('');
  constructor() {
    this.api.audit(undefined, 200).then((r) => this.rows.set(r)).catch((e) => this.error.set(e.message));
  }
}
