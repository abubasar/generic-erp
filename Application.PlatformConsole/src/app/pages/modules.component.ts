import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PlatformApi } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { ModuleDto, BusinessTemplateDto } from '../core/models';

@Component({
  standalone: true,
  imports: [FormsModule],
  styles: [`.edit .row > div { flex: 1 1 150px; }`],
  template: `
    <h1>Module catalog</h1>
    @if (error()) { <div class="err">{{ error() }}</div> }

    <h2>Modules</h2>
    <div class="card">
      <table>
        <thead><tr><th>Key</th><th>Name</th><th>Category</th><th>Depends on</th><th>Metered</th><th>Sort</th>@if (auth.hasRole('Admin')) { <th></th> }</tr></thead>
        <tbody>
          @for (m of modules(); track m.key) {
            <tr>
              <td><code>{{ m.key }}</code></td>
              <td>{{ m.name }}<br><span class="muted" style="font-size:12px">{{ m.description }}</span></td>
              <td><span class="pill mute">{{ m.category }}</span></td>
              <td class="muted">{{ m.dependsOn || '—' }}</td>
              <td>{{ m.isMetered ? 'yes' : '—' }}</td>
              <td>{{ m.sortOrder }}</td>
              @if (auth.hasRole('Admin')) { <td><button class="ghost" (click)="edit(m)">Edit</button></td> }
            </tr>
          }
        </tbody>
      </table>
    </div>

    @if (auth.hasRole('Admin')) {
      <div class="card edit" style="margin-top:12px">
        <h2>{{ form.key && known(form.key) ? 'Edit module' : 'New module' }}</h2>
        <div class="row">
          <div><label>Key</label><input [(ngModel)]="form.key" /></div>
          <div><label>Name</label><input [(ngModel)]="form.name" /></div>
          <div><label>Category</label>
            <select [(ngModel)]="form.category"><option>Business</option><option>Core</option><option>Addon</option></select>
          </div>
        </div>
        <div class="row">
          <div><label>Depends on</label><input [(ngModel)]="form.dependsOn" placeholder="inventory" /></div>
          <div><label>Permission group</label><input [(ngModel)]="form.permissionGroup" /></div>
          <div><label>Sort</label><input type="number" [(ngModel)]="form.sortOrder" /></div>
        </div>
        <label>Description</label><input [(ngModel)]="form.description" />
        <label style="display:flex;gap:6px;width:auto;margin-top:8px"><input type="checkbox" style="width:auto" [(ngModel)]="form.isMetered" /> Metered</label>
        <button style="margin-top:12px" (click)="save()">Save module</button>
        <button class="ghost" style="margin-left:8px" (click)="reset()">Clear</button>
      </div>
    }

    <h2>Business templates</h2>
    <div class="card">
      <table>
        <thead><tr><th>Key</th><th>Name</th><th>Industry profile</th><th>Default modules</th><th>Public</th></tr></thead>
        <tbody>
          @for (t of templates(); track t.key) {
            <tr>
              <td><code>{{ t.key }}</code></td><td>{{ t.name }}</td><td>{{ t.industryProfileKey }}</td>
              <td class="muted" style="font-size:12px">{{ t.defaultModuleKeys }}</td>
              <td>{{ t.isPublic ? 'yes' : 'no' }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
})
export class ModulesComponent {
  private api = inject(PlatformApi);
  auth = inject(AuthService);

  modules = signal<ModuleDto[]>([]);
  templates = signal<BusinessTemplateDto[]>([]);
  error = signal('');
  form: ModuleDto = this.blank();

  constructor() { this.load(); this.api.templates().then((t) => this.templates.set(t)); }

  private blank(): ModuleDto { return { key: '', name: '', category: 'Business', description: '', dependsOn: '', permissionGroup: '', isMetered: false, sortOrder: 0 }; }
  known(key: string): boolean { return this.modules().some((m) => m.key === key); }
  reset(): void { this.form = this.blank(); }
  edit(m: ModuleDto): void { this.form = { ...m }; }

  private load(): void { this.api.modules().then((m) => this.modules.set(m)).catch((e) => this.error.set(e.message)); }
  save(): void {
    this.error.set('');
    this.api.upsertModule(this.form).then(() => { this.load(); this.reset(); }).catch((e) => this.error.set(e.message));
  }
}
