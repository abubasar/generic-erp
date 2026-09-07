import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PlatformApi } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { TenantListItem, BusinessTemplateDto, PlanDto } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, FormsModule],
  styles: [`
    .toolbar { display: flex; gap: 10px; align-items: center; margin: 14px 0; }
    .toolbar input, .toolbar select { width: auto; }
    .new { margin-top: 14px; }
    .new .row > div { flex: 1 1 160px; }
  `],
  template: `
    <h1>Tenants</h1>
    <div class="toolbar">
      <input placeholder="Search name / code / sub-domain" [(ngModel)]="search" (keyup.enter)="load()" />
      <select [(ngModel)]="status" (change)="load()">
        <option value="">All statuses</option>
        <option>Active</option><option>Trial</option><option>PastDue</option><option>Suspended</option><option>Cancelled</option>
      </select>
      <button class="ghost" (click)="load()">Refresh</button>
      @if (auth.hasRole('Admin')) {
        <button style="margin-left:auto" (click)="showNew.set(!showNew())">{{ showNew() ? 'Cancel' : 'New tenant' }}</button>
      }
    </div>

    @if (showNew()) {
      <div class="card new">
        <h2>Create tenant</h2>
        <div class="row">
          <div><label>Code</label><input [(ngModel)]="nt.code" /></div>
          <div><label>Name</label><input [(ngModel)]="nt.name" /></div>
          <div><label>Sub-domain</label><input [(ngModel)]="nt.subdomain" /></div>
        </div>
        <div class="row">
          <div><label>Business template</label>
            <select [(ngModel)]="nt.businessTemplateKey">
              @for (t of templates(); track t.key) { <option [value]="t.key">{{ t.name }}</option> }
            </select>
          </div>
          <div><label>Plan (optional)</label>
            <select [(ngModel)]="nt.planKey">
              <option value="">— template defaults —</option>
              @for (p of plans(); track p.key) { <option [value]="p.key">{{ p.name }}</option> }
            </select>
          </div>
          <div><label>Currency</label><input [(ngModel)]="nt.currency" placeholder="BDT" /></div>
        </div>
        @if (error()) { <div class="err">{{ error() }}</div> }
        <button style="margin-top:14px" [disabled]="busy()" (click)="create()">{{ busy() ? 'Creating…' : 'Create' }}</button>
      </div>
    }

    @if (error() && !showNew()) { <div class="err">{{ error() }}</div> }
    <div class="card" style="margin-top:14px">
      <table>
        <thead><tr><th>Code</th><th>Name</th><th>Template</th><th>Status</th><th>Subscription</th><th>Modules</th><th>Created</th></tr></thead>
        <tbody>
          @for (t of rows(); track t.id) {
            <tr style="cursor:pointer" (click)="open(t)">
              <td>{{ t.code }}</td>
              <td>{{ t.name }}<br><span class="muted" style="font-size:12px">{{ t.subdomain }}</span></td>
              <td>{{ t.businessTemplateKey }}</td>
              <td><span class="pill" [class.ok]="t.status==='Active'" [class.warn]="t.status==='Trial'||t.status==='PastDue'" [class.bad]="t.status==='Suspended'||t.status==='Cancelled'">{{ t.status }}</span></td>
              <td class="muted">{{ t.subscriptionStatus }} @if (t.planKey) { · {{ t.planKey }} }</td>
              <td>{{ t.moduleCount }}</td>
              <td class="muted">{{ t.createdOn | date:'mediumDate' }}</td>
            </tr>
          } @empty { <tr><td colspan="7" class="muted">No tenants.</td></tr> }
        </tbody>
      </table>
    </div>
  `,
})
export class TenantsComponent {
  private api = inject(PlatformApi);
  private router = inject(Router);
  auth = inject(AuthService);

  rows = signal<TenantListItem[]>([]);
  templates = signal<BusinessTemplateDto[]>([]);
  plans = signal<PlanDto[]>([]);
  showNew = signal(false);
  busy = signal(false);
  error = signal('');
  search = '';
  status = '';
  nt: any = { code: '', name: '', subdomain: '', businessTemplateKey: 'feed', planKey: '', currency: 'BDT' };

  constructor() {
    this.load();
    this.api.templates().then((t) => { this.templates.set(t); if (t[0]) this.nt.businessTemplateKey = t[0].key; });
    this.api.plans().then((p) => this.plans.set(p));
  }

  load(): void {
    this.error.set('');
    this.api.tenants(this.search.trim() || undefined, this.status || undefined)
      .then((r) => this.rows.set(r)).catch((e) => this.error.set(e.message));
  }

  open(t: TenantListItem): void { this.router.navigate(['/tenants', t.id]); }

  async create(): Promise<void> {
    this.busy.set(true); this.error.set('');
    try {
      const body = { ...this.nt, subdomain: this.nt.subdomain || null, planKey: this.nt.planKey || null };
      const created = await this.api.createTenant(body);
      this.router.navigate(['/tenants', created.id]);
    } catch (e: any) {
      this.error.set(e.message);
    } finally {
      this.busy.set(false);
    }
  }
}
