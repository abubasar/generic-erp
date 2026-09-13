import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PlatformApi } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { TenantDetail, ModuleDto, PlanDto, ProvisioningStepStatus, PlatformInvoiceDto } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  styles: [`
    .head { display: flex; align-items: baseline; gap: 12px; }
    .cols { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; align-items: start; }
    .mod { display: flex; justify-content: space-between; align-items: center; padding: 7px 0; border-bottom: 1px solid var(--border); }
    .imp { background: var(--panel-2); padding: 10px; border-radius: 6px; font-family: monospace; font-size: 11px; word-break: break-all; margin-top: 8px; }
    dl { display: grid; grid-template-columns: auto 1fr; gap: 4px 12px; margin: 0; }
    dt { color: var(--muted); }
  `],
  template: `
    <p><a routerLink="/tenants">← Tenants</a></p>
    @if (error()) { <div class="err">{{ error() }}</div> }
    @if (t(); as d) {
      <div class="head">
        <h1>{{ d.name }}</h1>
        <span class="pill" [class.ok]="d.status==='Active'" [class.warn]="d.status==='Trial'" [class.bad]="d.status==='Suspended'||d.status==='Cancelled'">{{ d.status }}</span>
      </div>
      <p class="muted">{{ d.code }} @if (d.subdomain) { · {{ d.subdomain }} } · {{ d.businessTemplateKey }} · {{ d.currency }}</p>

      <div class="cols">
        <div class="card">
          <h2>Subscription</h2>
          <dl>
            <dt>Status</dt><dd>{{ d.subscription?.status || '—' }}</dd>
            <dt>Plan</dt><dd>{{ d.subscription?.planKey || '—' }}</dd>
            <dt>Period end</dt><dd>{{ d.subscription?.periodEnd ? (d.subscription!.periodEnd | date:'mediumDate') : '—' }}</dd>
          </dl>
          @if (auth.hasRole('Admin')) {
            <label>Set status / plan</label>
            <div class="row">
              <select [(ngModel)]="subStatus">
                <option>Active</option><option>Trial</option><option>PastDue</option><option>Suspended</option><option>Cancelled</option>
              </select>
              <select [(ngModel)]="subPlan">
                <option value="">no plan</option>
                @for (p of plans(); track p.key) { <option [value]="p.key">{{ p.name }}</option> }
              </select>
              <button (click)="saveSub()">Save</button>
            </div>
          }
        </div>

        <div class="card">
          <h2>Quotas</h2>
          @for (q of d.quotas; track q.key) {
            <div class="mod"><span>{{ q.key }}</span><span>{{ q.limit }}</span></div>
          } @empty { <p class="muted">No explicit quotas.</p> }
          @if (auth.hasRole('Admin')) {
            <div class="row" style="margin-top:10px">
              <input placeholder="key e.g. max_users" [(ngModel)]="qKey" />
              <input type="number" placeholder="limit" [(ngModel)]="qLimit" style="width:90px" />
              <button (click)="saveQuota()">Set</button>
            </div>
            <p class="muted" style="font-size:12px">Limit 0 removes the quota.</p>
          }
        </div>
      </div>

      <h2>Modules</h2>
      <div class="card">
        @for (m of allModules(); track m.key) {
          <div class="mod">
            <span>{{ m.name }} <span class="muted" style="font-size:12px">{{ m.key }}@if (m.category === 'Core') { · core }</span></span>
            <label style="display:flex;align-items:center;gap:8px;width:auto;margin:0">
              <span class="pill" [class.ok]="stateOf(m.key)==='Active'" [class.mute]="stateOf(m.key)!=='Active'">{{ stateOf(m.key) }}</span>
              @if (auth.hasRole('Admin')) {
                <input type="checkbox" style="width:auto" [checked]="stateOf(m.key)==='Active'"
                       [disabled]="m.category==='Core'" (change)="toggle(m.key, $any($event.target).checked)" />
              }
            </label>
          </div>
        }
      </div>

      <h2>Provisioning</h2>
      <div class="card">
        <div class="row" style="gap:6px;flex-wrap:wrap;align-items:center">
          @for (s of provSteps(); track s.stepKey) {
            <span class="pill" [class.ok]="s.status==='Done'" [class.bad]="s.status==='Failed'" [class.mute]="s.status!=='Done' && s.status!=='Failed'"
                  [title]="s.error || ''">{{ s.stepKey }}@if (s.attempts > 1) { ·{{ s.attempts }} }</span>
          } @empty { <span class="muted">No provisioning record.</span> }
        </div>
        @if (auth.hasRole('Admin')) {
          <button style="margin-top:10px" (click)="rerunProvisioning()">Re-run provisioning</button>
          <span class="muted" style="font-size:12px;margin-left:8px">idempotent — only retries failed / missing steps</span>
        }
      </div>

      <h2>Invoices</h2>
      <div class="card">
        @if (invoices().length) {
          <table>
            <thead><tr><th>Number</th><th>Period</th><th>Amount</th><th>Status</th><th>Due</th><th>Paid</th><th></th></tr></thead>
            <tbody>
              @for (inv of invoices(); track inv.id) {
                <tr>
                  <td>{{ inv.number }}</td>
                  <td>{{ inv.periodStart | date:'mediumDate' }} – {{ inv.periodEnd | date:'mediumDate' }}</td>
                  <td>{{ inv.currency }} {{ inv.amount | number:'1.0-0' }}</td>
                  <td><span class="pill" [class.ok]="inv.status==='Paid'" [class.warn]="inv.status==='Unpaid'" [class.mute]="inv.status==='Void'">{{ inv.status }}</span></td>
                  <td>{{ inv.dueOn ? (inv.dueOn | date:'mediumDate') : '—' }}</td>
                  <td>{{ inv.paidOn ? (inv.paidOn | date:'mediumDate') : '—' }}</td>
                  <td>
                    @if (auth.hasRole('Admin') && inv.status === 'Unpaid') {
                      <button (click)="markInvoicePaid(inv.id)">Mark paid</button>
                      <button class="ghost" (click)="voidInvoice(inv.id)">Void</button>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        } @else {
          <p class="muted">No invoices yet.</p>
        }

        @if (auth.hasRole('Admin')) {
          <label>New invoice</label>
          <div class="row" style="align-items:flex-end">
            <div>
              <label style="margin:0 0 3px">Period start</label>
              <input type="date" [(ngModel)]="invPeriodStart" style="width:150px" />
            </div>
            <div>
              <label style="margin:0 0 3px">Period end</label>
              <input type="date" [(ngModel)]="invPeriodEnd" style="width:150px" />
            </div>
            <div>
              <label style="margin:0 0 3px">Amount</label>
              <input type="number" [(ngModel)]="invAmount" style="width:110px" />
            </div>
            <div>
              <label style="margin:0 0 3px">Due date</label>
              <input type="date" [(ngModel)]="invDueOn" style="width:150px" />
            </div>
            <div style="flex:1">
              <label style="margin:0 0 3px">Note</label>
              <input [(ngModel)]="invNote" placeholder="optional" />
            </div>
            <button (click)="createInvoice()">Create</button>
          </div>
        }
      </div>

      @if (auth.hasRole('Support')) {
        <h2>Support</h2>
        <div class="card">
          <button (click)="impersonate()">Get an "act as tenant" token</button>
          <p class="muted" style="font-size:12px">Issues a short-lived tenant access token (audited). Paste it into the tenant app to reproduce an issue.</p>
          @if (impToken()) { <div class="imp">acting as {{ impActor() }}<br>{{ impToken() }}</div> }
        </div>
      }
    }
  `,
})
export class TenantDetailComponent {
  private api = inject(PlatformApi);
  private route = inject(ActivatedRoute);
  auth = inject(AuthService);

  id = this.route.snapshot.paramMap.get('id')!;
  t = signal<TenantDetail | null>(null);
  allModules = signal<ModuleDto[]>([]);
  plans = signal<PlanDto[]>([]);
  error = signal('');
  subStatus = 'Active';
  subPlan = '';
  qKey = '';
  qLimit: number | null = null;
  impToken = signal('');
  impActor = signal('');
  provSteps = signal<ProvisioningStepStatus[]>([]);
  invoices = signal<PlatformInvoiceDto[]>([]);
  invPeriodStart = '';
  invPeriodEnd = '';
  invAmount: number | null = null;
  invDueOn = '';
  invNote = '';

  constructor() {
    this.reload();
    this.loadCatalog();
    this.loadProvisioning();
    this.loadInvoices();
  }

  private loadCatalog(): void {
    this.api.modules().then((m) => this.allModules.set(m)).catch((e) => this.error.set(e.message));
    this.api.plans().then((p) => this.plans.set(p)).catch((e) => this.error.set(e.message));
  }

  private loadProvisioning(): void {
    this.api.provisioningStatus(this.id).then((s) => this.provSteps.set(s)).catch(() => {});
  }

  rerunProvisioning(): void {
    this.error.set('');
    this.api.provision(this.id).then((r) => { this.provSteps.set(r.steps); this.reload(); }).catch((e) => this.error.set(e.message));
  }

  private reload(): void {
    this.api.tenant(this.id).then((d) => {
      this.t.set(d);
      this.subStatus = d.subscription?.status || 'Active';
      this.subPlan = d.subscription?.planKey || '';
    }).catch((e) => this.error.set(e.message));
  }

  stateOf(key: string): string {
    return this.t()?.modules.find((m) => m.moduleKey === key)?.status ?? 'Off';
  }

  private run(p: Promise<TenantDetail>): void {
    this.error.set('');
    p.then((d) => this.t.set(d)).catch((e) => this.error.set(e.message));
  }

  toggle(key: string, enabled: boolean): void { this.run(this.api.toggleModule(this.id, key, enabled)); }
  saveSub(): void { this.run(this.api.setSubscription(this.id, { status: this.subStatus, planKey: this.subPlan || null })); }
  saveQuota(): void {
    if (!this.qKey.trim()) return;
    this.run(this.api.setQuota(this.id, this.qKey.trim(), Number(this.qLimit) || 0));
    this.qKey = ''; this.qLimit = null;
  }
  async impersonate(): Promise<void> {
    this.error.set('');
    try {
      const r = await this.api.impersonate(this.id);
      this.impToken.set(r.accessToken);
      this.impActor.set(r.actingAs);
    } catch (e: any) { this.error.set(e.message); }
  }

  private loadInvoices(): void {
    this.api.invoices(this.id).then((i) => this.invoices.set(i)).catch((e) => this.error.set(e.message));
  }

  async createInvoice(): Promise<void> {
    this.error.set('');
    if (!this.invPeriodStart || !this.invPeriodEnd || !this.invAmount) {
      this.error.set('Period start, period end and amount are required.');
      return;
    }
    try {
      await this.api.createInvoice(this.id, {
        periodStart: this.invPeriodStart,
        periodEnd: this.invPeriodEnd,
        amount: Number(this.invAmount),
        dueOn: this.invDueOn || null,
        note: this.invNote || null,
      });
      this.invPeriodStart = ''; this.invPeriodEnd = ''; this.invAmount = null; this.invDueOn = ''; this.invNote = '';
      this.loadInvoices();
    } catch (e: any) { this.error.set(e.message); }
  }

  markInvoicePaid(invoiceId: string): void {
    this.error.set('');
    this.api.markInvoicePaid(this.id, invoiceId).then(() => this.loadInvoices()).catch((e) => this.error.set(e.message));
  }

  voidInvoice(invoiceId: string): void {
    this.error.set('');
    this.api.voidInvoice(this.id, invoiceId).then(() => this.loadInvoices()).catch((e) => this.error.set(e.message));
  }
}
