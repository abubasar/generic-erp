import { AbstractControl, ValidatorFn, ValidationErrors } from "@angular/forms";

// Custom validator function
export function conditionalRequiredValidator(
  condition: () => boolean
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (condition()) {
      // If the condition is true, check if the value is empty
      const isInvalid =
        control.value === null ||
        control.value === undefined ||
        control.value === "";
      return isInvalid ? { required: true } : null;
    } else {
      // If the condition is false, no validation error
      return null;
    }
  };
}
