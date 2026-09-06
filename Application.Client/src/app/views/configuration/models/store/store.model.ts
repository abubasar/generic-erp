import { InventoryType } from "../inventory-type/inventory-type.model";

export interface Store {
  id?: string;
  code?: string;
  name: string;
  depoChargePerKg: number;
  inventoryTypeId: string;
  inventoryType?: InventoryType;
}
