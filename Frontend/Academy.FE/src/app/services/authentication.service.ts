import { Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { Router } from "@angular/router";
import {
  GoogleLoginProvider,
  SocialAuthService,
  SocialUser,
} from "@abacritt/angularx-social-login";
import { BehaviorSubject, from, Observable } from "rxjs";
import { jwtDecode } from "jwt-decode";
import { AuthenticatedUser } from "../shared/dto/authenticated-user.dto";
import { ErrorMessages } from "../shared/constants/error-messages";
import { JwtHelperService } from "@auth0/angular-jwt";
import { isPlatformBrowser } from "@angular/common";

const jwtHelper = new JwtHelperService();

@Injectable({
  providedIn: "root",
})
export class AuthenticationService {
  socialUser: any;
  private _isLoggedIn = false;
  private _session = null;
  public isFirstLoggedIn: boolean = false;

  isLoggedOut: BehaviorSubject<boolean> = new BehaviorSubject(true);
  isLoggedOut$ = this.isLoggedOut.asObservable();
  setLogout(status: boolean) {
    this.isLoggedOut.next(status);
  }

  authUser: BehaviorSubject<AuthenticatedUser> = new BehaviorSubject({});
  authUser$ = this.authUser.asObservable();
  setAuthenticateUser(user: AuthenticatedUser) {
    this.authUser.next(user);
  }

  constructor(
    private router: Router,
    private socialAuthService: SocialAuthService,
    @Inject(PLATFORM_ID) private platformId: Object // Inject PLATFORM_ID to check if it's browser or server
  ) {}

  signInWithGoogle(): Observable<SocialUser> {
    return from(this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID));
  }

  signOut(): void {
    this.socialAuthService.signOut().then(
      () => {},
      (value: any) => {
        console.log("Sign out successful..", value);
        localStorage.removeItem("user");
        localStorage.removeItem("authToken");
        localStorage.removeItem("idToken");
        this.setLogout(true);
        this.router.navigate(["login"]);
      }
    );
  }

  public get photoUrl(): string {
    const strUser = localStorage.getItem("user") ?? "";
    if (strUser !== "") {
      const user = JSON.parse(strUser);
      return user.photoUrl;
    } else return "../../../../assets/img/profile.jpg";
  }

  public get accessToken(): string {
    return localStorage.getItem("authToken") ?? "";
  }

  public get accessTokenExpiry(): any {
    return jwtHelper.decodeToken(this.accessToken).exp;
  }

  public get idToken(): string {
    return localStorage.getItem("idToken") ?? "";
  }

  public get isLoggedIn(): boolean {
    if (!this.accessToken || !this.idToken) return false;

    const isAccessTokenExpired = jwtHelper.isTokenExpired(this.accessToken);
    const isIdTokenExpired = jwtHelper.isTokenExpired(this.idToken);

    return !isIdTokenExpired;
  }

  public get userDetails():AuthenticatedUser{
    const token = this.fetchToken();
    if (!token) {
      throw new Error(ErrorMessages.UserDetailFromToken);
    }

    const decodedToken: any = jwtDecode(token);
    if (!decodedToken) {
      throw new Error(ErrorMessages.UserDetailFromToken);
    }

    const parsedData = JSON.parse(decodedToken["claimjson"]);

    const authenticatedUser: AuthenticatedUser = {
      id: parsedData.Id,
      globerEmail: parsedData.GloberEmail,
      name: parsedData.Name,
      roles: parsedData.Roles.map((role: any) => ({
        roleId: role.RoleId,
        roleName: role.RoleName,
        roleAssignment: role.RoleAssignment,
        displayName: role.DisplayName,
      })),
      community: parsedData.Community,
      ecosystem: parsedData.Ecosystem,
      careerMentorEmail: parsedData.CareerMentorEmail,
      userGexLeaderEmail: parsedData.UserGexLeaderEmail,
      project: parsedData.Project,
      client: parsedData.Client,
      seniorityId: parsedData.SeniorityId,
      seniority: parsedData.Seniority,
      isAuthenticated: parsedData.IsAuthenticated,
      gexLeaders: parsedData.GexLeaders,
      photoUrl: this.photoUrl,
    };
    return authenticatedUser;
  }

  fetchToken(): string {
    if (isPlatformBrowser(this.platformId)) {
      const token: string = localStorage.getItem("authToken") ?? "";

      if (!token) {
        this.signOut();
        return "";
      }

      return token;
    }
    return "";
  }

  fetchIdToken() {
    const token: string = localStorage.getItem("idToken") ?? "";

    if (!token) {
      this.signOut();
      return "";
    }

    return token;
  }

  decodeTokenandReadClaim(): AuthenticatedUser | null {
    try {
      const token = this.fetchToken();
      if (!token) {
        throw new Error(ErrorMessages.UserDetailFromToken);
      }

      const decodedToken: any = jwtDecode(token);
      if (!decodedToken) {
        throw new Error(ErrorMessages.UserDetailFromToken);
      }

      const parsedData = JSON.parse(decodedToken["claimjson"]);

      const authenticatedUser: AuthenticatedUser = {
        id: parsedData.Id,
        globerEmail: parsedData.GloberEmail,
        name: parsedData.Name,
        roles: parsedData.Roles.map((role: any) => ({
          roleId: role.RoleId,
          roleName: role.RoleName,
          roleAssignment: role.RoleAssignment,
          displayName: role.DisplayName,
        })),
        community: parsedData.Community,
        ecosystem: parsedData.Ecosystem,
        careerMentorEmail: parsedData.CareerMentorEmail,
        userGexLeaderEmail: parsedData.UserGexLeaderEmail,
        project: parsedData.Project,
        client: parsedData.Client,
        seniorityId: parsedData.SeniorityId,
        seniority: parsedData.Seniority,
        isAuthenticated: parsedData.IsAuthenticated,
        gexLeaders: parsedData.GexLeaders,
        photoUrl: this.photoUrl,
      };
      this.setAuthenticateUser(authenticatedUser);
      return authenticatedUser;
    } catch (error) {
      console.error(ErrorMessages.UserDetailFromToken, error);
      return null;
    }
  }

  getUserSessionDetails(){
    const token = this.fetchToken();
      if (!token) {
        throw new Error(ErrorMessages.UserDetailFromToken);
      }

      const decodedToken: any = jwtDecode(token);
      if (!decodedToken) {
        throw new Error(ErrorMessages.UserDetailFromToken);
      }

      const strUser = localStorage.getItem("user") ?? "";
      if (strUser !== "" && strUser != undefined) {
        const user = JSON.parse(strUser);
        return user;
      } 
  }


}
