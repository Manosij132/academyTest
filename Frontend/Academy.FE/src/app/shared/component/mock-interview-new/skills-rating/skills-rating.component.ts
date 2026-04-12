import { Component, Input } from "@angular/core";
import { MatGridListModule } from "@angular/material/grid-list";
import { TitleCaseCustomPipe } from "@shared/component/custom-pipes/title-case.pipe";

@Component({
  selector: "app-skills-rating",
  standalone: true,
  imports: [MatGridListModule,TitleCaseCustomPipe],
  templateUrl: "./skills-rating.component.html",
  styleUrl: "./skills-rating.component.css",
})
export class SkillsRatingComponent {
  @Input() skillData: any;
}
