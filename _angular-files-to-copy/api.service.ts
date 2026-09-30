import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface AuthResponse { token: string; name: string; role: string; }

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = 'http://localhost:5080/api';

  register = (b: any) => this.http.post<AuthResponse>(`${this.base}/auth/register`, b);
  login = (b: any) => this.http.post<AuthResponse>(`${this.base}/auth/login`, b);
  doctors = () => this.http.get<any[]>(`${this.base}/doctors`);
  slots = (id: number, date: string) => this.http.get<string[]>(`${this.base}/doctors/${id}/slots`, { params: { date } });
  book = (doctorId: number, startsAt: string) => this.http.post(`${this.base}/appointments`, { doctorId, startsAt });
  mine = () => this.http.get<any[]>(`${this.base}/appointments/mine`);
  cancel = (id: number) => this.http.delete(`${this.base}/appointments/${id}`);
}
