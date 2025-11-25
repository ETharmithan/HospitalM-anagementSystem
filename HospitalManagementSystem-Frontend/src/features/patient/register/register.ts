import { Component } from '@angular/core';
import { RegisterCreds } from '../../../types/user';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  protected creds = {} as RegisterCreds;
}
