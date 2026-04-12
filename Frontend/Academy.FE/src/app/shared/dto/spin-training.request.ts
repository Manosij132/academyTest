export class SpinTrainingRequest {
  force: boolean = false;
  ecosystem: number = 0;
  account: string = "";
  trainingAssignmentSrc: string = "";
  mapping: UserTrainingMapping[] = [];
  selectedTraning: any = [];
}

export class UserTrainingMapping {
  userId: number = 0;
  userEmail: string = "";
  seniorityId: number = 0;
  seniority: string = "";
  userImage: string = "";
  trainings: ecosystemTraining[] = [];
  parent: boolean = false;
  selectedTraning: any = [];
  selected: boolean = true;
}

export class KeyValuePair<Tkey, Tval> {
  Key: Tkey | undefined;
  Value: Tval | undefined;
}

export class ecosystemTraining {
  isMvP: boolean = false;
  seniority: string = "";
  seniorityId: number = 0;
  trainingCompletionHours: number = 0;
  trainingDescription: string = "";
  trainingId: number = 0;
  trainingLink: string = "";
  trainingName: string = "";
  checked: boolean = false;
}
export class User {
  firstname: any;
  lastname: any;
  selected: any;
}

export interface Option {
  name: string;
  value: number;
  checked: boolean;
}

// export class User {
//   constructor(public firstname: string, public lastname: string, public selected?: boolean) {
//     if (selected === undefined) selected = false;
//   }
// }
