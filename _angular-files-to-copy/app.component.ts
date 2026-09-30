import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from './api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
    <header><h1>CareNet</h1>
      @if (user()) { <span>{{ user()!.name }} <button (click)="logout()">Logout</button></span> }
    </header>
    <main>
      @if (msg) { <p class="msg">{{ msg }}</p> }
      @if (!user()) {
        <section class="card">
          <h2>{{ isRegister ? 'Create account' : 'Sign in' }}</h2>
          @if (isRegister) { <input placeholder="Name" [(ngModel)]="form.name" /> }
          <input placeholder="Email" [(ngModel)]="form.email" />
          <input placeholder="Password" type="password" [(ngModel)]="form.password" />
          <button (click)="submit()">{{ isRegister ? 'Register' : 'Login' }}</button>
          <a (click)="isRegister = !isRegister">{{ isRegister ? 'Have an account? Login' : 'New here? Register' }}</a>
        </section>
      }
      <h2>Doctors</h2>
      <div class="grid">
        @for (d of doctors(); track d.id) {
          <div class="card" [class.sel]="selected?.id === d.id">
            <h3>{{ d.name }}</h3><small>{{ d.specialty }}</small><p>{{ d.bio }}</p>
            <button (click)="pick(d)">View slots</button>
          </div>
        }
      </div>
      @if (selected) {
        <section class="card">
          <h3>Book with {{ selected.name }}</h3>
          <input type="date" [(ngModel)]="date" (change)="loadSlots()" />
          @for (s of slots(); track s) {
            <button [disabled]="!user()" (click)="book(s)">{{ s | date: 'shortTime' }}</button>
          }
          @if (!user()) { <p>Login to book.</p> }
        </section>
      }
      @if (user()) {
        <h2>My appointments</h2>
        @for (a of mine(); track a.id) {
          <p>{{ a.doctorName }} - {{ a.startsAt | date: 'medium' }} <button (click)="cancel(a.id)">Cancel</button></p>
        } @empty { <p>No appointments yet.</p> }
      }
    </main>`,
  styles: [`
    header { display:flex; justify-content:space-between; align-items:center; padding:12px 24px; background:#0f766e; color:#fff; }
    main { max-width:900px; margin:auto; padding:16px; font-family:system-ui,sans-serif; }
    .grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(250px,1fr)); gap:12px; }
    .card { border:1px solid #ddd; border-radius:10px; padding:14px; margin-bottom:12px; display:flex; flex-direction:column; gap:8px; }
    .sel { border-color:#0f766e; }
    .msg { background:#ecfeff; padding:8px; border-radius:6px; }
    button { background:#0f766e; color:#fff; border:0; border-radius:6px; padding:6px 12px; cursor:pointer; margin:2px; }
    button:disabled { opacity:.5; } input { padding:6px; } a { cursor:pointer; color:#0f766e; }`],
})
export class AppComponent {
  api = inject(ApiService);
  user = signal<{ name: string; role: string } | null>(JSON.parse(localStorage.getItem('user') || 'null'));
  doctors = signal<any[]>([]);
  slots = signal<string[]>([]);
  mine = signal<any[]>([]);
  selected: any = null;
  date = '';
  msg = '';
  isRegister = false;
  form = { name: '', email: '', password: '' };

  constructor() {
    this.api.doctors().subscribe((d) => this.doctors.set(d));
    if (this.user()) this.loadMine();
  }
  loadMine() { this.api.mine().subscribe((m) => this.mine.set(m)); }
  submit() {
    const req = this.isRegister ? this.api.register(this.form) : this.api.login(this.form);
    req.subscribe({
      next: (a) => {
        localStorage.setItem('token', a.token);
        const u = { name: a.name, role: a.role };
        localStorage.setItem('user', JSON.stringify(u));
        this.user.set(u); this.msg = ''; this.loadMine();
      },
      error: (e) => (this.msg = e.error?.message ?? 'Please check your input'),
    });
  }
  logout() { localStorage.clear(); this.user.set(null); this.mine.set([]); }
  pick(d: any) { this.selected = d; this.slots.set([]); this.loadSlots(); }
  loadSlots() {
    if (this.selected && this.date) this.api.slots(this.selected.id, this.date).subscribe((s) => this.slots.set(s));
  }
  book(s: string) {
    this.api.book(this.selected.id, s).subscribe({
      next: () => { this.msg = 'Appointment booked!'; this.loadSlots(); this.loadMine(); },
      error: (e) => { this.msg = e.error?.message ?? 'Booking failed'; this.loadSlots(); },
    });
  }
  cancel(id: number) { this.api.cancel(id).subscribe(() => { this.loadMine(); this.loadSlots(); }); }
}
