import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [FormsModule],
  styles: [`
    .wrap { min-height: 100vh; display: grid; place-items: center; }
    .box { width: 340px; }
    .box h1 { text-align: center; margin-bottom: 2px; }
    .box p { text-align: center; margin-top: 0; }
  `],
  template: `
    <div class="wrap">
      <div class="box card">
        <h1>Platform Console</h1>
        <p class="muted">Operator sign-in</p>
        <label>Email</label>
        <input type="email" [(ngModel)]="email" (keyup.enter)="submit()" autocomplete="username" />
        <label>Password</label>
        <input type="password" [(ngModel)]="password" (keyup.enter)="submit()" autocomplete="current-password" />
        @if (error()) { <div class="err">{{ error() }}</div> }
        <button style="width:100%;margin-top:16px" [disabled]="busy()" (click)="submit()">
          {{ busy() ? 'Signing in…' : 'Sign in' }}
        </button>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  busy = signal(false);
  error = signal('');

  async submit(): Promise<void> {
    if (this.busy()) return;
    this.error.set('');
    this.busy.set(true);
    try {
      await this.auth.login(this.email.trim(), this.password);
      this.router.navigateByUrl('/dashboard');
    } catch (e: any) {
      this.error.set(e?.message || 'Sign-in failed');
    } finally {
      this.busy.set(false);
    }
  }
}
