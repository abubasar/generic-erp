import { Region } from "../region/region.model";

export interface Zone{
    id: string;
    name: string;
    regionId: string;
    region?: Region;
  }