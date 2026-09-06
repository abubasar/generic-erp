import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { ResultPageComponent } from "./result-page/result-page.component";

const routes: Routes = [
  {
    path: "",
    component: ResultPageComponent,
    data: {
      module: "Menu Search",
      pageTitle: "menu search",
      breadcrumb: {
        title: "menu search",
        url: "search",
      },
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SearchViewRoutingModule {}
