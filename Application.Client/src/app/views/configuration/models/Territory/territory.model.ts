import { Area } from "../area/area.model";

export interface Territory {
  id: string;
  name: string;
  areaId: string;
  area?: Area;
}
