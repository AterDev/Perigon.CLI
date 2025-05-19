import { Injectable } from "@angular/core";
// import { AuthResult } from "../share/models/auth/auth-result.model";

@Injectable({ providedIn: 'root' })
export class LoginService {
  isLogin = false;
  isAdmin = false;
  userName?: string | null = null;
  id?: string | null = null;
  constructor() {
    this.updateUserLoginState();
  }

  saveLoginState(username: string, token: string): void {
    this.isLogin = true;
    this.userName = username;
    localStorage.setItem("username", username);
    localStorage.setItem("accessToken", token);
  }

  updateUserLoginState(): void {
    const username = localStorage.getItem('username');
    const token = localStorage.getItem('accessToken');
    if (token && username) {
      this.userName = username;
      this.isLogin = true;
    } else {
      this.isLogin = false;
    }
  }
  logout(): void {
    localStorage.clear();
    this.isLogin = false;
  }
}
