import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { JwtAuthService } from '../services/auth/jwt-auth.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[];

  constructor(private viewContainerRef: ViewContainerRef, 
    private templateRef: TemplateRef<any>, 
    private authService: JwtAuthService) {}

    ngOnInit(): void {
      const isAuthorized = this.authService.isRoleAuthorized('Role', this.appHasRole);
      if (!isAuthorized) {
        this.viewContainerRef.clear();
      } else {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }
    }
}
