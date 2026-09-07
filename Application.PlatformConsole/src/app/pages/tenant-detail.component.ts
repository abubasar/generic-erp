import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PlatformApi } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { TenantDetail, ModuleDto, PlanDto } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
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

  constructor() {
    this.reload();
    this.api.modules().then((m) => this.allModules.set(m));
    this.api.plans().then((p) => this.plans.set(p));
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
}
