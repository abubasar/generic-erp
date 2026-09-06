import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { SearchViewRoutingModule } from "./search-view-routing.module";
import { ResultPageComponent } from "./result-page/result-page.component";
import { MatCardModule } from "@angular/material/card";
import { MatTableModule } from "@angular/material/table";
import { MatPaginatorModule } from "@angular/material/paginator";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedPipesModule } from "app/shared/pipes/shared-pipes.module";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";

@NgModule({
  declarations: [ResultPageComponent],
  imports: [
    MatCardModule,
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    SearchViewRoutingModule,
    SharedComponentsModule,
    SharedPipesModule,
    SharedDirectivesModule,
  ],
})
export class SearchViewModule {}
