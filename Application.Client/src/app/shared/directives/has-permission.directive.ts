import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { JwtAuthService } from '../services/auth/jwt-auth.service';

@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective implements OnInit{
  @Input() appHasPermission: string[];

  constructor(private viewContainerRef: ViewContainerRef, 
    private templateRef: TemplateRef<any>, 
    private authService: JwtAuthService) {}

    ngOnInit(): void {
      const isAuthorized = this.authService.isPermissionAuthorized('Permission', this.appHasPermission);
      if (!isAuthorized) {
        this.viewContainerRef.clear();
      } else {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }
    }
}
