import { FormControl } from "@angular/forms";

export interface ManageSeniorityForm {
  id: FormControl<number>;
  name: FormControl<string>;
  level: FormControl<string>;
  isActive: FormControl<boolean>;
}
