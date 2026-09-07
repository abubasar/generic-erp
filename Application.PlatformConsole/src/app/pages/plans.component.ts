import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PlatformApi } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { PlanDto, PriceBookDto } from '../core/models';

@Component({
  standalone: true,
  imports: [FormsModule, DatePipe],
  styles: [`
    .edit .row > div { flex: 1 1 150px; }
    .price-row { display: grid; grid-template-columns: 90px 1fr 120px 120px; gap: 8px; margin-bottom: 6px; }
  `],
  template: `
    <h1>Plans &amp; pricing</h1>
    @if (error()) { <div class="err">{{ error() }}</div> }

    <h2>Plans</h2>
    <div class="card">
      <table>
        <thead><tr><th>Key</th><th>Name</th><th>Modules</th><th>Quotas</th><th>Public</th><th>Active</th>@if (auth.hasRole('Admin')) { <th></th> }</tr></thead>
        <tbody>
          @for (p of plans(); track p.key) {
            <tr>
              <td><code>{{ p.key }}</code></td>
              <td>{{ p.name }}</td>
              <td class="muted" style="font-size:12px">{{ p.moduleKeys }}</td>
              <td class="muted" style="font-size:12px">{{ p.quotas }}</td>
              <td>{{ p.isPublic ? 'yes' : 'no' }}</td>
              <td>{{ p.isActive ? 'yes' : 'no' }}</td>
              @if (auth.hasRole('Admin')) { <td><button class="ghost" (click)="editPlan(p)">Edit</button></td> }
            </tr>
          } @empty { <tr><td colspan="7" class="muted">No plans yet.</td></tr> }
        </tbody>
      </table>
    </div>

    @if (auth.hasRole('Admin')) {
      <div class="card edit" style="margin-top:12px">
        <h2>{{ pForm.key && known(pForm.key) ? 'Edit plan' : 'New plan' }}</h2>
        <div class="row">
          <div><label>Key</label><input [(ngModel)]="pForm.key" /></div>
          <div><label>Name</label><input [(ngModel)]="pForm.name" /></div>
          <div><label>Sort</label><input type="number" [(ngModel)]="pForm.sortOrder" /></div>
        </div>
        <label>Module keys (comma-separated)</label>
        <input [(ngModel)]="pForm.moduleKeys" />
        <label>Quotas (key=limit,comma-separated)</label>
        <input [(ngModel)]="pForm.quotas" placeholder="max_users=10,max_branches=2" />
        <div class="row" style="margin-top:8px">
          <label style="display:flex;gap:6px;width:auto"><input type="checkbox" style="width:auto" [(ngModel)]="pForm.isPublic" /> Public</label>
          <label style="display:flex;gap:6px;width:auto"><input type="checkbox" style="width:auto" [(ngModel)]="pForm.isActive" /> Active</label>
        </div>
        <button style="margin-top:12px" (click)="savePlan()">Save plan</button>
        <button class="ghost" style="margin-left:8px" (click)="resetPlan()">Clear</button>
      </div>
    }

    <h2>Price books</h2>
    <div class="card">
      @for (b of books(); track b.id) {
        <div style="border-bottom:1px solid var(--border);padding:10px 0">
          <div class="row" style="justify-content:space-between">
            <b>v{{ b.version }} · {{ b.currency }}</b>
            <span class="pill" [class.ok]="b.status==='Published'" [class.mute]="b.status!=='Published'">{{ b.status }}</span>
          </div>
          <div class="muted" style="font-size:12px">
            {{ b.note }} @if (b.publishedOn) { · published {{ b.publishedOn | date:'short' }} }
          </div>
          <div style="margin-top:6px">
            @for (e of b.entries; track e.id) {
              <span class="pill mute" style="margin-right:6px">{{ e.itemType }}:{{ e.itemKey }} = {{ e.monthlyPrice }}/mo</span>
            }
          </div>
          @if (auth.hasRole('Admin') && b.status === 'Draft') {
            <button style="margin-top:8px" (click)="publish(b.id)">Publish</button>
          }
        </div>
      } @empty { <p class="muted">No price books.</p> }

      @if (auth.hasRole('Admin')) {
        <h2>New draft price book</h2>
        <div class="row"><div><label>Currency</label><input [(ngModel)]="pbCurrency" /></div><div style="flex:2"><label>Note</label><input [(ngModel)]="pbNote" /></div></div>
        <label>Entries</label>
        @for (row of pbEntries; track $index) {
          <div class="price-row">
            <select [(ngModel)]="row.itemType"><option value="plan">plan</option><option value="module">module</option></select>
            <input placeholder="key" [(ngModel)]="row.itemKey" />
            <input type="number" placeholder="monthly" [(ngModel)]="row.monthlyPrice" />
            <input type="number" placeholder="unit (opt)" [(ngModel)]="row.unitPrice" />
          </div>
        }
        <button class="ghost" (click)="pbEntries.push({ itemType:'plan', itemKey:'', monthlyPrice:0, unitPrice:null })">+ row</button>
        <button style="margin-left:8px" (click)="createBook()">Create draft</button>
      }
    </div>
  `,
})
export class PlansComponent {
  private api = inject(PlatformApi);
  auth = inject(AuthService);

  plans = signal<PlanDto[]>([]);
  books = signal<PriceBookDto[]>([]);
  error = signal('');
  pForm: PlanDto = this.blankPlan();
  pbCurrency = 'BDT';
  pbNote = '';
  pbEntries: any[] = [{ itemType: 'plan', itemKey: '', monthlyPrice: 0, unitPrice: null }];

  constructor() { this.loadPlans(); this.loadBooks(); }

  private blankPlan(): PlanDto { return { key: '', name: '', description: '', moduleKeys: '', quotas: '', isPublic: true, isActive: true, sortOrder: 0 }; }
  known(key: string): boolean { return this.plans().some((p) => p.key === key); }
  resetPlan(): void { this.pForm = this.blankPlan(); }
  editPlan(p: PlanDto): void { this.pForm = { ...p }; }

  private loadPlans(): void { this.api.plans().then((p) => this.plans.set(p)).catch((e) => this.error.set(e.message)); }
  private loadBooks(): void { this.api.priceBooks().then((b) => this.books.set(b)).catch((e) => this.error.set(e.message)); }

  savePlan(): void {
    this.error.set('');
    this.api.upsertPlan(this.pForm).then(() => { this.loadPlans(); this.resetPlan(); }).catch((e) => this.error.set(e.message));
  }
  publish(id: string): void {
    this.api.publishPriceBook(id).then(() => this.loadBooks()).catch((e) => this.error.set(e.message));
  }
  createBook(): void {
    this.error.set('');
    const entries = this.pbEntries.filter((e) => e.itemKey?.trim())
      .map((e) => ({ ...e, monthlyPrice: Number(e.monthlyPrice) || 0, unitPrice: e.unitPrice != null && e.unitPrice !== '' ? Number(e.unitPrice) : null }));
    this.api.createPriceBook({ currency: this.pbCurrency, note: this.pbNote, entries })
      .then(() => { this.loadBooks(); this.pbEntries = [{ itemType: 'plan', itemKey: '', monthlyPrice: 0, unitPrice: null }]; this.pbNote = ''; })
      .catch((e) => this.error.set(e.message));
  }
}
