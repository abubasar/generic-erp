import { AbstractControl, ValidationErrors } from "@angular/forms";

export function inFinancialYearValidator(startDate: Date, endDate: Date) {
  return (control: AbstractControl): ValidationErrors | null => {
    const date = new Date(control.value);

    // Reset the time part to 00:00:00 for comparison
    const start = new Date(startDate.setHours(0, 0, 0, 0));
    const end = new Date(endDate.setHours(0, 0, 0, 0));
    const current = new Date(date.setHours(0, 0, 0, 0));

    if (!control.value || (current >= start && date <= end)) {
      return null;
    }

    // const formattedStart = this.datePipe.transform(startDate, "mediumDate");
    // const formattedEnd = this.datePipe.transform(endDate, "mediumDate");

    return {
      inFinancialYear: {
        valid: false,
        // message: `Please choose a date within the financial year period: ${formattedStart} - ${formattedEnd}.`,
      },
    };
  };
}
