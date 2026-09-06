import { Injectable } from "@angular/core";
import * as moment from "moment";

@Injectable({
  providedIn: "root",
})
export class DateTimeFormatService {
  constructor() {}
  getDateFormat(date): string {
    return moment(date).format("L");
  }
  getDateAndTime(date) {
    const momentDate = moment(date);
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
