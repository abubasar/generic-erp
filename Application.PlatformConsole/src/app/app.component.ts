import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'pc-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  styles: [`
    .shell { display: grid; grid-template-columns: 210px 1fr; min-height: 100vh; }
    aside { background: #0b1220; border-right: 1px solid var(--border); padding: 16px 12px; }
    .brand { font-weight: 800; letter-spacing: .02em; padding: 6px 8px 14px; }
    nav a { display: block; padding: 8px 10px; border-radius: 6px; color: var(--muted); margin-bottom: 2px; }
    nav a:hover { background: var(--panel-2); text-decoration: none; }
    nav a.active { background: var(--panel-2); color: var(--text); }
    main { padding: 22px 26px; max-width: 1100px; }
    .who { position: absolute; top: 14px; right: 22px; font-size: 12px; color: var(--muted); }
    .who button { margin-left: 10px; padding: 4px 10px; }
  `],
  template: `
    @if (auth.isAuthenticated()) {
      <div class="shell">
        <aside>
          <div class="brand">BUTS · Platform</div>
          <nav>
            <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
            <a routerLink="/tenants" routerLinkActive="active">Tenants</a>
            <a routerLink="/plans" routerLinkActive="active">Plans &amp; pricing</a>
            <a routerLink="/modules" routerLinkActive="active">Module catalog</a>
            <a routerLink="/audit" routerLinkActive="active">Audit log</a>
          </nav>
        </aside>
        <main>
          <div class="who">
            {{ auth.admin()?.email }} · <b>{{ auth.admin()?.role }}</b>
            <button class="ghost" (click)="auth.logout()">Sign out</button>
          </div>
          <router-outlet />
        </main>
      </div>
    } @else {
      <router-outlet />
    }
  `,
})
export class AppComponent {
  auth = inject(AuthService);
}
