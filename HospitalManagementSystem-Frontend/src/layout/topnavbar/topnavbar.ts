import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-topnavbar',
  imports: [CommonModule],
  templateUrl: './topnavbar.html',
  styleUrl: './topnavbar.css',
})
export class Topnavbar {
  @Input() userName = 'Alex Johnson';
  @Input() patientId = '738496';
  @Input() avatarUrl = ''; // pass image url if available
}
