export class ManageTrainingRequest {
  ecosystem: number = 0;
  skillTrainingMapping: SkillTrainingMapping[] = [];
}

export class SkillTrainingMapping {
  skillName: string = "";
  skillId: number = 0;
  description: string = "";
  seniorityProficiencyMapping: SeniorityProficiencyMapping[] = [];
  trainings: Training[] = [];
  isMVP: boolean = false;
}

export class SeniorityProficiencyMapping {
  seniorityId: number = 0;
  proficiency: number = 0;
}

export class Training {
  trainingId: number = 0;
  trainingName: string = "";
  description: string = "";
  uri: string = "";
}
