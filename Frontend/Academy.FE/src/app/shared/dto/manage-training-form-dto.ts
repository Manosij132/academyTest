import { FormArray, FormControl, FormGroup } from "@angular/forms";
export interface Proficiency {
  seniorityId: number;
  proficiencyValue: number;
}
export interface ManageTrainingForm {
  ecosystemId: FormControl<number>;
  skillId: FormControl<number>;
  trainingId: FormControl<number>;
  trainingName: FormControl<string>;
  trainingDescription: FormControl<string>;
  trainingUrl: FormControl<string>;
  trainingCompletionHours: FormControl<number>;
  ismvp: FormControl<boolean>;
  expectedProficiency: FormArray<
    FormGroup<{
      seniorityId: FormControl<number>;
      proficiencyValue: FormControl<number>;
    }>
  >;
}

export interface ManageTraining {
  ecosystemId: number;
  skillId: number;
  trainingId: number;
  trainingName: string;
  trainingDescription: string;
  trainingUrl: string;
  trainingCompletionHours: number;
  ismvp: boolean;
  expectedProficiency: ExpectedProficiency[];
}

export interface ExpectedProficiency {
  seniorityId: number;
  proficiencyId: number;
}
