export class CreateSkillRequest {
  SkillName: string = "";
  SkillDescription: string = "";
  IsActive: boolean = true;
  Mandatory: boolean = false;
  Grouping: string = "";
  CategoryId: number = 0;
  Specification: string = "";
}
