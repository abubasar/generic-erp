import { InventoryType } from "../inventory-type/inventory-type.model";

export interface ProductType {
  id: string;
  inventoryTypeId: string;
  name: string;
  inventoryType?: InventoryType;
}
