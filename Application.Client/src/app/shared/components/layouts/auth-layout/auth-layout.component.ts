import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-auth-layout',
    templateUrl: './auth-layout.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class AuthLayoutComponent implements OnInit {

  constructor(
    public translate: TranslateService,
  ) { 
    // Translator init
    const browserLang: string = translate.getBrowserLang();
    translate.use(browserLang.match(/en|fr/) ? browserLang : 'en');
  }

  ngOnInit() {
  }

}
