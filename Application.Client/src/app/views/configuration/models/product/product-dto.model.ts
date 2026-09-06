export interface ProductDTO {
  id: string;
  code?: string;
  name: string;
  inventoryTypeId: string;
  productTypeId: string;
  categoryId?: string;
  genericId?: string;
  composition?: string;
  countryId?: string;
  packSizeId?: string;
  packSize?: string;
  isPurchaseProduct: boolean;
  isSaleProduct: boolean;
  measurementUnitId: string;
  mrp: number;
  salePrice: number;
  purchasePrice: number;
  alertQuantity: number;
  bagWeight: number;
  manufacturerId?: string;
  vatPercentage: number;
  lastPurchaseRate?: number;
}
