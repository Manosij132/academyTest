import { CommonModule } from "@angular/common";
import {
  Component
} from "@angular/core";
import { DomSanitizer, SafeUrl } from "@angular/platform-browser";
import { RouterModule } from "@angular/router";
import { AuthenticationService } from "../../../services/authentication.service";

@Component({
  selector: "app-header",
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: "./header.component.html",
  styleUrl: "./header.component.scss",
})
export class HeaderComponent {
  userDetail: any = {};
  safeProfileImageUrl: SafeUrl = "";
  constructor(
    private authenticationService: AuthenticationService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.userDetail = this.authenticationService.userDetails;
    this.safeProfileImageUrl = this.sanitizer.bypassSecurityTrustUrl(
      this.userDetail.photoUrl
    );
  }

  logout() {
    this.authenticationService.signOut();
  }
}
