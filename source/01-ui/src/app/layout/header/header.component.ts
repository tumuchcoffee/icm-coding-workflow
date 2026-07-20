import { Component, output } from '@angular/core';
import { Button } from 'primeng/button';
import { Avatar } from 'primeng/avatar';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [Button, Avatar, RouterLink],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  menuToggle = output<void>();
}