import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { from, map, Observable, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  protected baseUrl: string | null;
  constructor(
    protected http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    if (baseUrl.endsWith('/')) {
      this.baseUrl = baseUrl.slice(0, -1);
    } else {
      this.baseUrl = baseUrl;
    }
  }

  protected request<T = any>(method: string, path: string, body?: any): Observable<T> {
    const url = this.baseUrl + path;
    return this.http.request(method, url, {
      headers: this.getHeaders(),
      body,
      responseType: 'blob',
      observe: 'response'
    }).pipe(
      switchMap((resp: HttpResponse<Blob>) => {
        const contentType = (resp.headers.get('Content-Type') || '').toLowerCase();
        const disposition = resp.headers.get('Content-Disposition') || '';
        const blob = resp.body as Blob;

        const isAttachment = /attachment/i.test(disposition);
        const isBinaryType = this.isBinaryContentType(contentType);
        const treatAsFile = isAttachment || isBinaryType;

        if (treatAsFile) {
          return from(Promise.resolve(blob as unknown as T));
        }
        return from(blob.text()).pipe(
          map(text => this.parseAuto<T>(text, contentType))
        );
      })
    );
  }

  private parseAuto<T>(text: string, contentType: string): T {
    if (contentType.includes('application/json') || contentType.includes('text/json')) {
      return JSON.parse(text) as T;
    }
    return text as unknown as T;
  }

  private isBinaryContentType(ct: string): boolean {
    if (!ct) return false;
    return (
      ct.startsWith('application/octet-stream') ||
      ct.startsWith('application/pdf') ||
      ct.startsWith('application/zip') ||
      ct.startsWith('application/vnd') ||
      ct.startsWith('image/') ||
      ct.startsWith('video/') ||
      ct.startsWith('audio/') ||
      ct.startsWith('font/') ||
      ct.includes('application/msword') ||
      ct.includes('application/excel')
    );
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
      Accept: 'application/json, text/plain, */*',
      Authorization: 'Bearer ' + localStorage.getItem('accessToken')
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
