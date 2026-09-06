import { FormGroup, FormControl, Validators, FormArray } from "@angular/forms";
export function compareFieldsValidator(field1Name: string, field2Name: string) {
  return (formGroup: FormGroup) => {
    //field1=stock quantity,field2=quantity
    const field1 = formGroup.get(field1Name) as FormControl;
    const field2 = formGroup.get(field2Name) as FormControl;
     if (field1.value == 0) return { field1Zero: true };
    if (field1.value && field2.value && field1.value < field2.value) {
      return { fieldsNotGreater: true };
    }

    return null;
  };
}
