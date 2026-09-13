import { Injectable } from "@angular/core";
import * as moment from "moment";

@Injectable({
  providedIn: "root",
})
export class DateTimeFormatService {
  constructor() {}
  getDateFormat(date): string {
    if (!date) return "";
    const momentDate = moment(date);
    return momentDate.isValid() ? momentDate.format("L") : "";
  }
  getDateAndTime(date) {
    if (!date) return "";
    const momentDate = moment(date);
    if (!momentDate.isValid()) return "";
    const formattedDate = momentDate.format("L");
    const formattedTime = momentDate.format("LT");
    return `${formattedDate} ${formattedTime}`;
  }
  getPresentDate() {
    let today = new Date();
    return new Date(today.getFullYear(), today.getMonth(), today.getDate());
  }
  getDateDynamically(days: number) {
    let today = new Date();
    return new Date(
      today.getFullYear(),
      today.getMonth(),
      today.getDate() + days
    );
  }
}
