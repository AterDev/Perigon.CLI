import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  protected baseUrl: string | null;
  constructor(
    protected http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
    // private oidcSecurityService: OidcSecurityService
  ) {
    if (baseUrl.endsWith('/')) {
      this.baseUrl = baseUrl.slice(0, -1);
    } else {
      this.baseUrl = baseUrl;
    }
  }

  protected request<R>(method: string, path: string, body?: any): Observable<R> {
    const url = this.baseUrl + path;
    const options = {
      headers: this.getHeaders(),
      body
    };
    return this.http.request<R>(method, url, options);
  }

  protected downloadFile(method: string, path: string, body?: any): Observable<Blob> {
    const url = this.baseUrl + path;
    const options = {
      responseType: 'blob' as 'blob',
      headers: this.getHeaders(),
      body,
    };
    return this.http.request(method, url, options);
  }

  protected openFile(blob: Blob, filename: string) {
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  protected getHeaders(): HttpHeaders {
    return new HttpHeaders({
      Accept: 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem('accessToken'),
    });
  }
  public isMobile(): boolean {
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
