export interface ApiResult<T> {
  data: T;
  succeeded: boolean;
  statusCode: number;
  message?: string;
  error?: string;
  exception?: string;
}

export interface PlatformAdminInfo { id: string; email: string; name: string; role: string; }
export interface PlatformLoginResult { accessToken: string; expiresOn: string; admin: PlatformAdminInfo; }

export interface TenantListItem {
  id: string; code: string; name: string; subdomain?: string; businessTemplateKey?: string;
  status: string; subscriptionStatus?: string; planKey?: string; moduleCount: number; createdOn: string;
}
export interface SubscriptionInfo { id: string; planKey?: string; status: string; periodStart: string; periodEnd: string; trialEndsOn?: string; }
export interface TenantModuleInfo { moduleKey: string; moduleName: string; status: string; activatedOn: string; expiresOn?: string; }
export interface QuotaInfo { key: string; limit: number; }
export interface TenantDetail {
  id: string; code: string; name: string; subdomain?: string; customDomain?: string;
  businessTemplateKey?: string; status: string; currency?: string; businessType: number;
  dbConnectionKey?: string; createdOn: string;
  subscription?: SubscriptionInfo; modules: TenantModuleInfo[]; quotas: QuotaInfo[];
}

export interface ModuleDto { key: string; name: string; category: string; description?: string; dependsOn?: string; permissionGroup?: string; isMetered: boolean; sortOrder: number; }
export interface BusinessTemplateDto { key: string; name: string; description?: string; defaultModuleKeys: string; industryProfileKey: string; isPublic: boolean; sortOrder: number; }
export interface PlanDto { key: string; name: string; description?: string; moduleKeys: string; quotas: string; isPublic: boolean; isActive: boolean; sortOrder: number; }

export interface PriceBookEntryDto { id: string; itemType: string; itemKey: string; monthlyPrice: number; unitPrice?: number; }
export interface PriceBookDto { id: string; version: number; currency: string; status: string; publishedOn?: string; note?: string; entries: PriceBookEntryDto[]; }

export interface ModuleAdoption { moduleKey: string; moduleName: string; tenantCount: number; }
export interface AuditEntryDto { id: string; adminEmail: string; action: string; tenantId?: string; detail?: string; createdOn: string; }
export interface PlatformUsageSummary {
  tenantsTotal: number; tenantsActive: number; tenantsTrial: number; tenantsSuspended: number;
  estimatedMrr: number; currency: string; moduleAdoption: ModuleAdoption[]; recentActivity: AuditEntryDto[];
}

export interface ImpersonateResult { accessToken: string; refreshToken: string; tenantId: string; tenantName: string; actingAs: string; }

export interface ProvisioningStepStatus { stepKey: string; status: string; error?: string; attempts: number; completedOn?: string; }
export interface ProvisioningResult { tenantId: string; complete: boolean; ownerUsername?: string; ownerTempPassword?: string; steps: ProvisioningStepStatus[]; }
export interface CreateTenantResult { tenant: TenantDetail; provisioning: ProvisioningResult; }

export interface PricingLine { label: string; detail: string; amount: number; }
export interface PricingQuote { currency: string; priceBookVersion?: number; lines: PricingLine[]; monthlyTotal: number; pricingConfigured: boolean; }
