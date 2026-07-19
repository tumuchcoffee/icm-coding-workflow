import { Component, input, output } from '@angular/core';
import { Sidebar } from 'primeng/sidebar';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [Sidebar],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss'],
})
export class MenuComponent {
  isOpen = input<boolean>(false);
  closed = output<void>();
}