import { Directive, ElementRef, OnInit, Renderer2 } from "@angular/core";
import { UserRole } from "../constants/app.constants";
import { AuthenticationService } from "./../../services/authentication.service";

@Directive({
  selector: "[appPrivilegedUser]",
  standalone: true,
})
export class PrivilegedUserDirective implements OnInit {
  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private authService: AuthenticationService
  ) {}

  ngOnInit(): void {
    this.updateVisibility();
  }

  private updateVisibility() {
    const loggedInUserRole = this.authService.userDetails?.roles?.[0]?.roleName;
    if (
      loggedInUserRole === UserRole.User ||
      loggedInUserRole === undefined
    ) {
      this.renderer.setStyle(this.el.nativeElement, "display", "none");
    } else {
      this.renderer.removeStyle(this.el.nativeElement, "display"); // Remove the style to revert to default
    }
  }
}
