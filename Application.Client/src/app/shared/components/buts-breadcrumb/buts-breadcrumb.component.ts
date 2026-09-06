import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { RoutePartsService } from "app/shared/services/route-parts.service";
import { Subscription, filter } from "rxjs";

@Component({
  selector: "app-buts-breadcrumb",
  templateUrl: "./buts-breadcrumb.component.html",
  styleUrls: ["./buts-breadcrumb.component.scss"],
})
export class ButsBreadcrumbComponent implements OnInit {
  routerEventSub: Subscription;
  constructor(
    private activatedRoute: ActivatedRoute,
    private routePartsService: RoutePartsService,
    private router: Router
  ) {
    this.routerEventSub = this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((routeChange) => {
        let routeData = this.routePartsService.generateRouteParts(
          this.activatedRoute.snapshot
        )[0];
        this.data = {
          child: [
            { title: routeData.module },
            {
              title: routeData.breadcrumb?.title,
              url: routeData?.breadcrumb?.url,
            },
            { title: routeData?.pageTitle },
          ],
        };
      });
  }
  data: any;

  ngOnInit() {}
  ngOnDestroy() {
    if (this.routerEventSub) {
      this.routerEventSub.unsubscribe();
    }
  }
}
