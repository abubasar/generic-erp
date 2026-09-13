import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  ApiResult, BusinessTemplateDto, PlanDto, PricingRequest, PricingQuote,
  PublicSignupRequest, CreateTenantResult,
} from './models';

@Injectable({ providedIn: 'root' })
export class SignupApi {
  private http = inject(HttpClient);

  private async unwrap<T>(p: Promise<ApiResult<T>>): Promise<T> {
    const res = await p;
    if (!res || res.succeeded === false) {
      throw new Error(res?.message || res?.exception || 'Request failed');
    }
    return res.data;
  }
  private get<T>(url: string) { return this.unwrap(firstValueFrom(this.http.get<ApiResult<T>>(url))); }
  private post<T>(url: string, body?: unknown) { return this.unwrap(firstValueFrom(this.http.post<ApiResult<T>>(url, body ?? {}))); }

  businessTemplates() { return this.get<BusinessTemplateDto[]>('/api/signup/business-templates'); }
  plans() { return this.get<PlanDto[]>('/api/signup/plans'); }
  quote(request: PricingRequest) { return this.post<PricingQuote>('/api/signup/pricing/quote', request); }
  signUp(request: PublicSignupRequest) { return this.post<CreateTenantResult>('/api/signup/tenants', request); }
}
