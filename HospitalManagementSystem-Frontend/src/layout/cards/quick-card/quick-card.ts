import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-quick-card',
  imports: [],
  templateUrl: './quick-card.html',
  styleUrl: './quick-card.css',
})
export class QuickCard {
  @Input() icon = 'add_box';
  @Input() title = 'Title';
  @Input() description = '';
  @Input() actionText = 'Action';
}
