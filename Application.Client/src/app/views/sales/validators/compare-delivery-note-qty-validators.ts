import { FormControl, FormGroup } from "@angular/forms";
export function compareDeliveryNoteQtyValidator(
  field1Name: string,
  field2Name: string,
  field3Name: string
) {
  return (formGroup: FormGroup) => {
    //field1=delivered qty,field2=delivery bag qty,field3=ordered bag qty
    const field1 = formGroup.get(field1Name) as FormControl;
    const field2 = formGroup.get(field2Name) as FormControl;
    const field3 = formGroup.get(field3Name) as FormControl;
    if (
      //field1.value &&
      //field2.value &&
      //field3.value &&
      field3.value <
      field1.value + field2.value
    ) {
      //alert('fdfdfdf')
      return { deliveryQtyNotGreater: true };
    }
    return null;
  };
}
