import { Component, input } from '@angular/core';

@Component({
  selector: 'app-detail-pane',
  standalone: true,
  templateUrl: './detail-pane.component.html',
  styleUrls: ['./detail-pane.component.scss'],
})
export class DetailPaneComponent {
  isOpen = input<boolean>(false);
}