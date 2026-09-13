// Mirrors Application.Api/Controllers/MeController.cs's MeResponse.
// GET /api/me returns this shape directly — no {data, error, ...} envelope.
export interface MeResponse {
  tenantId: string;
  userId: string;
  userName: string;
  businessType: number;
  businessTemplateKey: string;
  subscriptionStatus: string;
  modules: string[];
  quotas: { [key: string]: number };
}
