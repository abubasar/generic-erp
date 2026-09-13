import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignupApi } from './core/signup-api.service';
import {
  BusinessTemplateDto, PlanDto, PricingQuote, CreateTenantResult, parseQuotas,
} from './core/models';

const STEP_LABELS = ['Business', 'Plan', 'Account', 'Review', 'Start'];

@Component({
  selector: 'su-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    .page { min-height: 100vh; display: flex; flex-direction: column; align-items: center; padding: 40px 16px 60px; }
    .brand { font-weight: 800; font-size: 20px; margin-bottom: 28px; letter-spacing: -.01em; }
    .brand span { color: var(--accent); }
    .wizard { width: 100%; max-width: 560px; }
    .steps { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .steps .dot-wrap { display: flex; flex-direction: column; align-items: center; gap: 6px; flex: 1; position: relative; }
    .steps .dot-wrap:not(:last-child)::after {
      content: ""; position: absolute; top: 13px; left: 50%; width: 100%; height: 2px; background: var(--border); z-index: 0;
    }
    .steps .dot-wrap.done:not(:last-child)::after { background: var(--accent); }
    .dot {
      width: 26px; height: 26px; border-radius: 50%; background: #fff; border: 2px solid var(--border);
      display: grid; place-items: center; font-size: 12px; font-weight: 700; color: var(--muted); z-index: 1;
    }
    .dot-wrap.active .dot { border-color: var(--accent); color: var(--accent); }
    .dot-wrap.done .dot { background: var(--accent); border-color: var(--accent); color: #fff; }
    .step-label { font-size: 11px; color: var(--muted); }
    .dot-wrap.active .step-label { color: var(--text); font-weight: 600; }

    .actions { display: flex; justify-content: space-between; margin-top: 24px; }
    .actions .spacer { flex: 1; }

    .plan-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin: 6px 0 8px; }
    .plan-card {
      border: 2px solid var(--border); border-radius: 12px; padding: 14px 12px; cursor: pointer; text-align: center;
    }
    .plan-card.selected { border-color: var(--accent); background: #f5f4ff; }
    .plan-card .name { font-weight: 700; margin-bottom: 4px; }
    .plan-card .desc { font-size: 12px; color: var(--muted); }

    .stepper-row { display: flex; align-items: center; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid var(--border); }
    .stepper-row:last-child { border-bottom: none; }
    .stepper-row .label { font-size: 14px; }
    .stepper-row .hint { font-size: 12px; color: var(--muted); }
    .stepper { display: flex; align-items: center; gap: 10px; }
    .stepper button { width: 30px; height: 30px; padding: 0; border-radius: 8px; font-size: 16px; line-height: 1; }
    .stepper .count { width: 24px; text-align: center; font-weight: 600; }

    .quote-box { background: #f9fafb; border-radius: 10px; padding: 14px 16px; margin-top: 16px; }
    .quote-line { display: flex; justify-content: space-between; font-size: 13px; padding: 3px 0; }
    .quote-line .detail { color: var(--muted); }
    .quote-total { display: flex; justify-content: space-between; font-weight: 700; font-size: 16px; margin-top: 8px; padding-top: 8px; border-top: 1px solid var(--border); }

    .review-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid var(--border); font-size: 14px; }
    .review-row:last-child { border-bottom: none; }
    .review-row .k { color: var(--muted); }

    .center { text-align: center; }
    .big-icon { font-size: 40px; margin-bottom: 8px; }
    .creds-box { background: #f9fafb; border-radius: 10px; padding: 16px; margin: 16px 0; text-align: left; }
    .creds-box .row { display: flex; justify-content: space-between; padding: 4px 0; font-family: ui-monospace, monospace; font-size: 13px; }
    .prov-step { display: flex; align-items: center; gap: 8px; padding: 4px 0; font-size: 13px; }
    .prov-step .ok { color: var(--ok); }
  `],
  template: `
    <div class="page">
      <div class="brand">BUTS<span>ERP</span></div>
      <div class="wizard">
        @if (step() <= 4) {
          <div class="steps">
            @for (label of stepLabels; track label; let i = $index) {
              <div class="dot-wrap" [class.active]="step() === i + 1" [class.done]="step() > i + 1">
                <div class="dot">{{ step() > i + 1 ? '✓' : i + 1 }}</div>
                <div class="step-label">{{ label }}</div>
              </div>
            }
          </div>
        }

        <div class="card">
          <!-- Step 1: Business -->
          @if (step() === 1) {
            <h2>Tell us about your business</h2>
            <p class="muted">We'll use this to set things up for you.</p>

            <label class="req">Business name</label>
            <input [(ngModel)]="businessName" placeholder="e.g. Green Valley Traders" />

            <label class="req">What kind of business is it?</label>
            @if (loadingCatalog()) {
              <p class="muted">Loading…</p>
            } @else {
              <div class="plan-grid" style="grid-template-columns: repeat(2, 1fr);">
                @for (t of templates(); track t.key) {
                  <div class="plan-card" [class.selected]="businessTemplateKey() === t.key" (click)="selectTemplate(t.key)">
                    <div class="name">{{ t.name }}</div>
                    <div class="desc">{{ t.description }}</div>
                  </div>
                }
              </div>
            }

            @if (catalogError()) { <div class="err">{{ catalogError() }}</div> }

            <div class="actions">
              <span class="spacer"></span>
              <button [disabled]="!canLeaveStep1()" (click)="goToStep(2)">Continue</button>
            </div>
          }

          <!-- Step 2: Plan -->
          @if (step() === 2) {
            <h2>Choose a plan</h2>
            <p class="muted">You can change this later.</p>

            <div class="plan-grid">
              @for (p of plans(); track p.key) {
                <div class="plan-card" [class.selected]="planKey() === p.key" (click)="selectPlan(p.key)">
                  <div class="name">{{ p.name }}</div>
                  <div class="desc">{{ p.description }}</div>
                </div>
              }
            </div>

            <label style="margin-top:20px">Your team &amp; locations</label>
            <div class="stepper-row">
              <div>
                <div class="label">Users</div>
                <div class="hint">{{ includedQuotas()['max_users'] || 0 }} included in this plan</div>
              </div>
              <div class="stepper">
                <button class="ghost" (click)="adjust('users', -1)">−</button>
                <div class="count">{{ users }}</div>
                <button class="ghost" (click)="adjust('users', 1)">+</button>
              </div>
            </div>
            <div class="stepper-row">
              <div>
                <div class="label">Branches</div>
                <div class="hint">{{ includedQuotas()['max_branches'] || 0 }} included in this plan</div>
              </div>
              <div class="stepper">
                <button class="ghost" (click)="adjust('branches', -1)">−</button>
                <div class="count">{{ branches }}</div>
                <button class="ghost" (click)="adjust('branches', 1)">+</button>
              </div>
            </div>
            <div class="stepper-row">
              <div>
                <div class="label">POS terminals</div>
                <div class="hint">{{ includedQuotas()['max_pos_terminals'] || 0 }} included in this plan</div>
              </div>
              <div class="stepper">
                <button class="ghost" (click)="adjust('posTerminals', -1)">−</button>
                <div class="count">{{ posTerminals }}</div>
                <button class="ghost" (click)="adjust('posTerminals', 1)">+</button>
              </div>
            </div>

            @if (quoting()) {
              <p class="muted" style="margin-top:14px">Calculating price…</p>
            } @else if (quote(); as q) {
              <div class="quote-box">
                @for (line of q.lines; track line.label) {
                  <div class="quote-line">
                    <span>{{ line.label }} <span class="detail">— {{ line.detail }}</span></span>
                    <span>{{ line.amount > 0 ? (line.amount | number:'1.0-0') : 'included' }}</span>
                  </div>
                }
                <div class="quote-total">
                  <span>Total / month</span>
                  <span>{{ q.currency }} {{ q.monthlyTotal | number:'1.0-0' }}</span>
                </div>
              </div>
            }
            @if (quoteError()) { <div class="err">{{ quoteError() }}</div> }

            <div class="actions">
              <button class="ghost" (click)="goToStep(1)">Back</button>
              <button [disabled]="!planKey()" (click)="goToStep(3)">Continue</button>
            </div>
          }

          <!-- Step 3: Account -->
          @if (step() === 3) {
            <h2>Create your account</h2>
            <p class="muted">You'll sign in with this once you're set up.</p>

            <label class="req">Work email</label>
            <input type="email" [(ngModel)]="ownerEmail" (ngModelChange)="onEmailChange()" placeholder="you@business.com" />

            <label class="req">Username</label>
            <input [(ngModel)]="ownerUsername" placeholder="Chosen automatically from your email" />

            <label class="req">Password</label>
            <input type="password" [(ngModel)]="ownerPassword" placeholder="At least 8 characters" />

            <label class="req">Confirm password</label>
            <input type="password" [(ngModel)]="ownerPasswordConfirm" (keyup.enter)="canLeaveStep3() && goToStep(4)" />

            @if (step3Error()) { <div class="err">{{ step3Error() }}</div> }

            <div class="actions">
              <button class="ghost" (click)="goToStep(2)">Back</button>
              <button [disabled]="!canLeaveStep3()" (click)="goToStep(4)">Continue</button>
            </div>
          }

          <!-- Step 4: Review -->
          @if (step() === 4) {
            <h2>Review &amp; confirm</h2>

            <div class="review-row"><span class="k">Business</span><span>{{ businessName }}</span></div>
            <div class="review-row"><span class="k">Type</span><span>{{ selectedTemplateName() }}</span></div>
            <div class="review-row"><span class="k">Plan</span><span>{{ selectedPlanName() }}</span></div>
            <div class="review-row"><span class="k">Users / Branches / POS</span><span>{{ users }} / {{ branches }} / {{ posTerminals }}</span></div>
            <div class="review-row"><span class="k">Sign-in email</span><span>{{ ownerEmail }}</span></div>

            @if (quote(); as q) {
              <div class="quote-box">
                <div class="quote-total">
                  <span>Total / month</span>
                  <span>{{ q.currency }} {{ q.monthlyTotal | number:'1.0-0' }}</span>
                </div>
              </div>
            }
            <p class="muted" style="margin-top:14px;font-size:12px">
              By starting, you agree this is a trial setup you can change or cancel any time.
            </p>

            @if (submitError()) { <div class="err">{{ submitError() }}</div> }

            <div class="actions">
              <button class="ghost" [disabled]="submitting()" (click)="goToStep(3)">Back</button>
              <button [disabled]="submitting()" (click)="confirmAndStart()">
                {{ submitting() ? 'Setting up…' : 'Confirm & start' }}
              </button>
            </div>
          }

          <!-- Step 5: Provisioning / welcome -->
          @if (step() === 5 && result(); as r) {
            <div class="center">
              <div class="big-icon">🎉</div>
              <h2>You're all set, {{ businessName }}!</h2>
              <p class="muted">Here's what we did behind the scenes:</p>
            </div>

            @for (s of r.provisioning.steps; track s.stepKey) {
              <div class="prov-step">
                <span class="ok">✓</span>
                <span>{{ stepDisplayName(s.stepKey) }}</span>
              </div>
            }

            <div class="creds-box">
              <p style="margin-bottom:8px"><b>Your sign-in details</b> — save these somewhere safe.</p>
              <div class="row"><span>Username</span><span>{{ r.provisioning.ownerUsername }}</span></div>
              <div class="row"><span>Password</span><span>{{ ownerPassword }}</span></div>
            </div>

            <button style="width:100%" (click)="restart()">Done</button>
          }
        </div>
      </div>
    </div>
  `,
})
export class AppComponent implements OnInit {
  private api = inject(SignupApi);

  stepLabels = STEP_LABELS;
  step = signal(1);

  templates = signal<BusinessTemplateDto[]>([]);
  plans = signal<PlanDto[]>([]);
  loadingCatalog = signal(true);
  catalogError = signal('');

  // Step 1
  businessName = '';
  businessTemplateKey = signal('');

  // Step 2
  planKey = signal<string | null>(null);
  users = 0;
  branches = 0;
  posTerminals = 0;
  quote = signal<PricingQuote | null>(null);
  quoting = signal(false);
  quoteError = signal('');

  // Step 3
  ownerEmail = '';
  ownerUsername = '';
  ownerPassword = '';
  ownerPasswordConfirm = '';
  private usernameTouched = false;
  step3Error = signal('');

  // Step 4/5
  submitting = signal(false);
  submitError = signal('');
  result = signal<CreateTenantResult | null>(null);

  includedQuotas = computed(() => {
    const plan = this.plans().find((p) => p.key === this.planKey());
    return plan ? parseQuotas(plan.quotas) : {};
  });

  selectedTemplateName = computed(() =>
    this.templates().find((t) => t.key === this.businessTemplateKey())?.name ?? '');
  selectedPlanName = computed(() =>
    this.plans().find((p) => p.key === this.planKey())?.name ?? '');

  async ngOnInit(): Promise<void> {
    try {
      const [templates, plans] = await Promise.all([this.api.businessTemplates(), this.api.plans()]);
      this.templates.set(templates);
      this.plans.set(plans);
      if (templates.length) this.businessTemplateKey.set(templates[0].key);
    } catch (e: any) {
      this.catalogError.set(e?.message || 'Could not load signup options. Please try again shortly.');
    } finally {
      this.loadingCatalog.set(false);
    }
  }

  selectTemplate(key: string): void {
    this.businessTemplateKey.set(key);
  }

  canLeaveStep1(): boolean {
    return this.businessName.trim().length > 0 && this.businessTemplateKey().length > 0;
  }

  selectPlan(key: string): void {
    this.planKey.set(key);
    // Selecting/changing plan re-baselines the steppers to what's included.
    const plan = this.plans().find((p) => p.key === key);
    const q = plan ? parseQuotas(plan.quotas) : {};
    this.users = q['max_users'] ?? 0;
    this.branches = q['max_branches'] ?? 0;
    this.posTerminals = q['max_pos_terminals'] ?? 0;
    void this.refreshQuote();
  }

  adjust(field: 'users' | 'branches' | 'posTerminals', delta: number): void {
    const next = Math.max(0, this[field] + delta);
    this[field] = next;
    void this.refreshQuote();
  }

  // Guards against an in-flight request from a stale (users/branches/posTerminals)
  // combination overwriting the result of a newer one — a real risk here since
  // each stepper click fires its own request without waiting for the last.
  private quoteRequestId = 0;

  private async refreshQuote(): Promise<void> {
    if (!this.planKey()) return;
    const requestId = ++this.quoteRequestId;
    this.quoting.set(true);
    this.quoteError.set('');
    try {
      const q = await this.api.quote({
        planKey: this.planKey(),
        moduleKeys: [],
        users: this.users,
        branches: this.branches,
        posTerminals: this.posTerminals,
      });
      if (requestId !== this.quoteRequestId) return; // a newer request has since started
      this.quote.set(q);
    } catch (e: any) {
      if (requestId !== this.quoteRequestId) return;
      this.quoteError.set(e?.message || 'Could not calculate the price.');
    } finally {
      if (requestId === this.quoteRequestId) this.quoting.set(false);
    }
  }

  onEmailChange(): void {
    if (this.usernameTouched) return;
    this.ownerUsername = this.ownerEmail.trim().toLowerCase();
  }

  canLeaveStep3(): boolean {
    return (
      this.ownerEmail.trim().length > 3 &&
      this.ownerUsername.trim().length > 0 &&
      this.ownerPassword.length >= 8 &&
      this.ownerPassword === this.ownerPasswordConfirm
    );
  }

  goToStep(n: number): void {
    if (n === 4 && this.step() === 3) {
      this.step3Error.set('');
      if (!this.canLeaveStep3()) {
        this.step3Error.set(
          this.ownerPassword !== this.ownerPasswordConfirm
            ? 'Passwords do not match.'
            : 'Please fill in every field (password at least 8 characters).'
        );
        return;
      }
    }
    this.step.set(n);
  }

  stepDisplayName(stepKey: string): string {
    const names: Record<string, string> = {
      subdomain: 'Set up your web address',
      subscription: 'Started your subscription',
      'financial-year': 'Set up your financial year',
      'measurement-units': 'Set up units of measurement',
      company: 'Created your company profile',
      'default-store': 'Created your first store',
      'owner-role': 'Set up your owner permissions',
      'owner-user': 'Created your sign-in',
    };
    return names[stepKey] ?? stepKey;
  }

  async confirmAndStart(): Promise<void> {
    this.submitting.set(true);
    this.submitError.set('');
    try {
      const created = await this.api.signUp({
        businessName: this.businessName.trim(),
        businessTemplateKey: this.businessTemplateKey(),
        planKey: this.planKey(),
        currency: 'BDT',
        subdomain: null,
        users: this.users,
        branches: this.branches,
        posTerminals: this.posTerminals,
        ownerEmail: this.ownerEmail.trim(),
        ownerUsername: this.ownerUsername.trim(),
        ownerPassword: this.ownerPassword,
      });
      this.result.set(created);
      this.step.set(5);
    } catch (e: any) {
      this.submitError.set(e?.message || 'Something went wrong while setting up your account. Please try again.');
    } finally {
      this.submitting.set(false);
    }
  }

  restart(): void {
    location.reload();
  }
}
