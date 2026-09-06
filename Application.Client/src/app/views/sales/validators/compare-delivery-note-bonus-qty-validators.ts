import { FormControl, FormGroup } from "@angular/forms";
export function compareDeliveryNoteBonusQtyValidator(
  field1Name: string,
  field2Name: string,
  field3Name: string
) {
  return (formGroup: FormGroup) => {
    //field1=deliveredBonus,field2=delivery bonus bag qty,field3=bonus bag qty
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
      return { deliveryBonusQtyNotGreater: true };
    }
    return null;
  };
}
