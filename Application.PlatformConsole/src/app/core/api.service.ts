import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  ApiResult, TenantListItem, TenantDetail, ModuleDto, BusinessTemplateDto, PlanDto,
  PriceBookDto, PlatformUsageSummary, AuditEntryDto, ImpersonateResult,
} from './models';

@Injectable({ providedIn: 'root' })
export class PlatformApi {
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

  // Usage
  usage() { return this.get<PlatformUsageSummary>('/api/platform/usage'); }
  audit(tenantId?: string, take = 100) {
    const q = new URLSearchParams(); if (tenantId) q.set('tenantId', tenantId); q.set('take', String(take));
    return this.get<AuditEntryDto[]>(`/api/platform/audit?${q}`);
  }

  // Tenants
  tenants(search?: string, status?: string) {
    const q = new URLSearchParams(); if (search) q.set('search', search); if (status) q.set('status', status);
    return this.get<TenantListItem[]>(`/api/platform/tenants?${q}`);
  }
  tenant(id: string) { return this.get<TenantDetail>(`/api/platform/tenants/${id}`); }
  createTenant(body: unknown) { return this.post<TenantDetail>('/api/platform/tenants', body); }
  setStatus(id: string, status: string) { return this.post<TenantDetail>(`/api/platform/tenants/${id}/status`, { status }); }
  setSubscription(id: string, body: unknown) { return this.post<TenantDetail>(`/api/platform/tenants/${id}/subscription`, body); }
  toggleModule(id: string, moduleKey: string, enabled: boolean) { return this.post<TenantDetail>(`/api/platform/tenants/${id}/modules`, { moduleKey, enabled }); }
  setQuota(id: string, key: string, limit: number) { return this.post<TenantDetail>(`/api/platform/tenants/${id}/quotas`, { key, limit }); }
  impersonate(id: string) { return this.post<ImpersonateResult>(`/api/platform/tenants/${id}/impersonate`); }

  // Catalog
  modules() { return this.get<ModuleDto[]>('/api/platform/modules'); }
  upsertModule(dto: ModuleDto) { return this.post<ModuleDto>('/api/platform/modules', dto); }
  templates() { return this.get<BusinessTemplateDto[]>('/api/platform/business-templates'); }
  upsertTemplate(dto: BusinessTemplateDto) { return this.post<BusinessTemplateDto>('/api/platform/business-templates', dto); }
  plans() { return this.get<PlanDto[]>('/api/platform/plans'); }
  upsertPlan(dto: PlanDto) { return this.post<PlanDto>('/api/platform/plans', dto); }
  priceBooks() { return this.get<PriceBookDto[]>('/api/platform/price-books'); }
  createPriceBook(body: unknown) { return this.post<PriceBookDto>('/api/platform/price-books', body); }
  publishPriceBook(id: string) { return this.post<PriceBookDto>(`/api/platform/price-books/${id}/publish`); }
}
