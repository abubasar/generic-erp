import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { AutoFocusDirective } from "./auto-focus.directive";
import { CustomSpinnerDirective } from "./custom-spinner.directive";
import { DropdownAnchorDirective } from "./dropdown-anchor.directive";
import { DropdownLinkDirective } from "./dropdown-link.directive";
import { AppDropdownDirective } from "./dropdown.directive";
import { FontSizeDirective } from "./font-size.directive";
import { HasPermissionDirective } from "./has-permission.directive";
import { HasRoleDirective } from "./has-role.directive";
import { MatxHighlightDirective } from "./matx-highlight.directive";
import { MatxSideNavToggleDirective } from "./matx-side-nav-toggle.directive";
import {
  MatxSidenavHelperDirective,
  MatxSidenavTogglerDirective,
} from "./matx-sidenav-helper/matx-sidenav-helper.directive";
import { ScrollToDirective } from "./scroll-to.directive";

const directives = [
  AutoFocusDirective,
  FontSizeDirective,
  ScrollToDirective,
  AppDropdownDirective,
  CustomSpinnerDirective,
  DropdownAnchorDirective,
  DropdownLinkDirective,
  MatxHighlightDirective,
  MatxSideNavToggleDirective,
  MatxSidenavHelperDirective,
  MatxSidenavTogglerDirective,
  HasPermissionDirective,
  HasRoleDirective,
];

@NgModule({
  imports: [CommonModule],
  declarations: directives,
  exports: directives,
})
export class SharedDirectivesModule {}
