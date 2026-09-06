import { FormGroup, FormControl, Validators, FormArray } from "@angular/forms";
export function compareStockAdjustmentStockValidator(
  field1Name: string,
  field2Name: string
) {
  return (formGroup: FormGroup) => {
    //field1=currentStockQuantity,field2= adjustmentQty
    const field1 = formGroup.get(field1Name) as FormControl;
    const field2 = formGroup.get(field2Name) as FormControl;
    if (field1.value && field2.value && field1.value + field2.value < 0) {
      return { fieldsNotGreater: true };
    }

    return null;
  };
}
