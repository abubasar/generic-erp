import { Zone } from "../zone/zone.model";

export interface Area {
  id: string;
  name: string;
  zoneId: string;
  zone?: Zone;
}
