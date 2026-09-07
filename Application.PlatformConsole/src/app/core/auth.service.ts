import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ApiResult, PlatformLoginResult, PlatformAdminInfo } from './models';

const TOKEN_KEY = 'pc.token';
const ADMIN_KEY = 'pc.admin';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _token = signal<string | null>(read(TOKEN_KEY));
  private _admin = signal<PlatformAdminInfo | null>(readJson(ADMIN_KEY));

  readonly admin = this._admin.asReadonly();
  readonly isAuthenticated = computed(() => !!this._token());

  constructor(private http: HttpClient) {}

  get token(): string | null { return this._token(); }

  /** True when the current admin's role is at least `required` (Owner > Admin > Support > ReadOnly). */
  hasRole(required: 'Owner' | 'Admin' | 'Support' | 'ReadOnly'): boolean {
    const rank = (r?: string) => ({ Owner: 3, Admin: 2, Support: 1, ReadOnly: 0 }[r ?? ''] ?? -1);
    return rank(this._admin()?.role) >= rank(required);
  }

  async login(email: string, password: string): Promise<void> {
    const res = await firstValueFrom(
      this.http.post<ApiResult<PlatformLoginResult>>('/api/platform/auth/login', { email, password }),
    );
    if (!res.succeeded) throw new Error(res.message || 'Sign-in failed');
    this._token.set(res.data.accessToken);
    this._admin.set(res.data.admin);
    write(TOKEN_KEY, res.data.accessToken);
    write(ADMIN_KEY, JSON.stringify(res.data.admin));
  }

  logout(): void {
    this._token.set(null);
    this._admin.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ADMIN_KEY);
  }
}

function read(k: string): string | null { try { return localStorage.getItem(k); } catch { return null; } }
function readJson<T>(k: string): T | null { try { const v = localStorage.getItem(k); return v ? JSON.parse(v) as T : null; } catch { return null; } }
function write(k: string, v: string): void { try { localStorage.setItem(k, v); } catch { /* ignore */ } }
