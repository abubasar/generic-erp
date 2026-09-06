import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import { PerfectScrollbarModule } from "app/shared/components/perfect-scrollbar";
import { SharedDirectivesModule } from "../directives/shared-directives.module";
import { SharedPipesModule } from "../pipes/shared-pipes.module";
import { SearchModule } from "../search/search.module";
import { SharedMaterialModule } from "../shared-material.module";

// ONLY REQUIRED FOR **SIDE** NAVIGATION LAYOUT
import { SidebarSideComponent } from "./sidebar-side/sidebar-side.component";

// ONLY REQUIRED FOR **TOP** NAVIGATION LAYOUT
import { HeaderTopComponent } from "./header-top/header-top.component";

// ONLY FOR DEMO
import { CustomizerComponent } from "./customizer/customizer.component";

// ALWAYS REQUIRED
import { NgxSkeletonLoaderModule } from "ngx-skeleton-loader";
import { AppConfirmComponent } from "../services/app-confirm/app-confirm.component";
import { AppLoaderComponent } from "../services/app-loader/app-loader.component";
import { ButsBreadcrumbComponent } from "./buts-breadcrumb/buts-breadcrumb.component";
import { ButtonLoadingComponent } from "./button-loading/button-loading.component";
import { ConfirmDialogComponent } from "./confirm-dialog/confirm-dialog.component";
import { ConfirmReadyForGrnDialogComponent } from "./confirm-ready-for-grn-dialog/confirm-ready-for-grn-dialog.component";
import { FooterComponent } from "./footer/footer.component";
import { HeaderSideComponent } from "./header-side/header-side.component";
import { AdminLayoutComponent } from "./layouts/admin-layout/admin-layout.component";
import { AuthLayoutComponent } from "./layouts/auth-layout/auth-layout.component";
import { LoadingSkeletonComponent } from "./loading-skeleton/loading-skeleton.component";
import {
  MatxSidebarComponent,
  MatxSidebarTogglerDirective,
} from "./matx-sidebar/matx-sidebar.component";
import { NoDataFoundComponent } from "./no-data-found/no-data-found.component";
import { NotificationsComponent } from "./notifications/notifications.component";
import { SidenavComponent } from "./sidenav/sidenav.component";
import { TransactionalJournalDetailsComponent } from "./transactional-journal-details/transactional-journal-details.component";
import { MatTableModule } from "@angular/material/table";

const components = [
  HeaderTopComponent,
  SidenavComponent,
  NotificationsComponent,
  SidebarSideComponent,
  HeaderSideComponent,
  AdminLayoutComponent,
  AuthLayoutComponent,
  AppConfirmComponent,
  AppLoaderComponent,
  CustomizerComponent,
  ButtonLoadingComponent,
  MatxSidebarComponent,
  FooterComponent,
  MatxSidebarTogglerDirective,
  ConfirmDialogComponent,
  ConfirmReadyForGrnDialogComponent,
  TransactionalJournalDetailsComponent,
  LoadingSkeletonComponent,
  ButsBreadcrumbComponent,
  NoDataFoundComponent,
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    FlexLayoutModule,
    PerfectScrollbarModule,
    SearchModule,
    SharedPipesModule,
    SharedDirectivesModule,
    SharedMaterialModule,
    NgxSkeletonLoaderModule,
    MatTableModule,
  ],
  declarations: components,
  // entryComponents: [AppConfirmComponent, AppLoaderComponent, BottomSheetShareComponent],
  exports: components,
})
export class SharedComponentsModule {}
