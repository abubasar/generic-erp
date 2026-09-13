import { Injectable } from "@angular/core";
import { JwtAuthService } from "../auth/jwt-auth.service";

const PHARMACY = "pharmacy";
const FEED = "feed";

/*
  Client-side counterpart to Application.Core/Industry/IIndustryProfile.
  businessTemplateKey comes from the cached /api/me response (see
  JwtAuthService.getMe()) — the same source the backend resolves
  ITenantContext.BusinessTemplateKey from. Components should read
  isPharma/isFeed (or the helpers below) instead of comparing the legacy
  businessType JWT claim ('1'/'2') directly.
*/
@Injectable({
  providedIn: "root",
})
export class IndustryProfileService {
  constructor(private jwtAuth: JwtAuthService) {}

  private get key(): string | undefined {
    return this.jwtAuth.getMe()?.businessTemplateKey;
  }

  get isPharma(): boolean {
    return this.key === PHARMACY;
  }

  get isFeed(): boolean {
    return this.key === FEED;
  }

  // Pharma's primaryQuantity is already the sellable unit; Feed's is a bag
  // count that gets multiplied by Product.BagWeight to reach Kg.
  get quantityLabel(): string {
    return this.isFeed ? "Bag QTY" : "QTY";
  }

  get bonusQuantityLabel(): string {
    return this.isFeed ? "Bonus Bag QTY" : "Bonus QTY";
  }

  // Dashboard.ShowQuantityKpis / ShowValueKpis on the backend profile.
  get showQuantityKpis(): boolean {
    return this.isFeed;
  }

  get showValueKpis(): boolean {
    return this.isPharma;
  }

  // Reports.ProductLabel's pack-size suffix (Pharma) vs bag-weight suffix
  // (Feed) shown next to a product name in dropdowns/line items.
  productDisplaySuffix(product: {
    packSize?: { name?: string };
    bagWeight?: number;
    measurementUnit?: { name?: string };
  }): string {
    if (this.isFeed) {
      return product?.bagWeight != null && product?.measurementUnit?.name
        ? `${product.bagWeight} ${product.measurementUnit.name}`
        : "";
    }
    return product?.packSize?.name ?? "";
  }
}
