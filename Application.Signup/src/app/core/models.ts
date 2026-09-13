// Mirrors Application.Services/Services/Platform/PlatformContracts.cs and
// PricingEngine.cs — see api/signup/* in SignupController.cs.

export interface ApiResult<T> {
  data: T;
  succeeded: boolean;
  message?: string;
  exception?: string;
}

export interface BusinessTemplateDto {
  key: string;
  name: string;
  description?: string | null;
  defaultModuleKeys: string;
  industryProfileKey: string;
  isPublic: boolean;
  sortOrder: number;
}

export interface PlanDto {
  key: string;
  name: string;
  description?: string | null;
  moduleKeys: string;
  /** "key=limit,key=limit" — e.g. "max_users=8,max_branches=3,max_pos_terminals=2". */
  quotas: string;
  isPublic: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface PricingRequest {
  planKey: string | null;
  moduleKeys: string[];
  users: number;
  branches: number;
  posTerminals: number;
}

export interface PricingLine {
  label: string;
  detail: string;
  amount: number;
}

export interface PricingQuote {
  currency: string;
  priceBookVersion: number | null;
  lines: PricingLine[];
  monthlyTotal: number;
  pricingConfigured: boolean;
}

export interface PublicSignupRequest {
  businessName: string;
  businessTemplateKey: string;
  planKey: string | null;
  currency: string | null;
  subdomain: string | null;
  users: number;
  branches: number;
  posTerminals: number;
  ownerEmail: string;
  ownerUsername: string;
  ownerPassword: string;
}

export interface ProvisioningStepStatus {
  stepKey: string;
  status: string;
  error?: string | null;
  attempts: number;
  completedOn?: string | null;
}

export interface ProvisioningResult {
  tenantId: string;
  complete: boolean;
  ownerUsername?: string | null;
  ownerTempPassword?: string | null;
  steps: ProvisioningStepStatus[];
}

export interface TenantDetail {
  id: string;
  code: string;
  name: string;
  subdomain?: string | null;
}

export interface CreateTenantResult {
  tenant: TenantDetail;
  provisioning: ProvisioningResult;
}

/** Parses a Plan.Quotas string like "max_users=8,max_branches=3" into a lookup. */
export function parseQuotas(quotas: string): Record<string, number> {
  const out: Record<string, number> = {};
  for (const part of (quotas || '').split(',')) {
    const [key, value] = part.split('=');
    if (key && value && !isNaN(Number(value))) out[key.trim()] = Number(value);
  }
  return out;
}
