import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
// import { OidcSecurityService } from 'angular-auth-oidc-client';

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  public baseUrl: string | null;
  public isMobile = false;
  constructor(
    protected http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
    // private oidcSecurityService: OidcSecurityService
  ) {
    this.isMobile = this.isMoblie();
    if (baseUrl.endsWith('/')) {
      this.baseUrl = baseUrl.slice(0, -1);
    } else {
      this.baseUrl = baseUrl;
    }
  }

  request<R>(method: string, path: string, body?: any): Observable<R> {
    const url = this.baseUrl + path;
    const options = {
      headers: this.getHeaders(),
      body
    };
    return this.http.request<R>(method, url, options);
  }

  downloadFile(method: string, path: string, body?: any): Observable<Blob> {
    const url = this.baseUrl + path;
    const options = {
      responseType: 'blob' as 'blob',
      headers: this.getHeaders(),
      body,
    };
    return this.http.request(method, url, options);
  }

  openFile(blob: Blob, filename: string) {
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  getHeaders(): HttpHeaders {
    return new HttpHeaders({
      Accept: 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem('accessToken'),
    });
  }
  isMoblie(): boolean {
    const ua = navigator.userAgent;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
      return true;
    }
    return false;
  }
}
export interface ErrorResult {
  title: string;
  detail: string;
  status: number;
}
