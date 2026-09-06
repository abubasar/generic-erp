import { AfterViewInit, Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";

import { RoutePartsService } from "./shared/services/route-parts.service";

import { filter } from "rxjs/operators";
import { LayoutService } from "./shared/services/layout.service";
import { UILibIconService } from "./shared/services/ui-lib-icon.service";
import { UrlService } from "./shared/services/url.service";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"],
})
export class AppComponent implements OnInit, AfterViewInit {
  appTitle = "BUTS ERP";
  pageTitle = "";
  previousUrl: string = null;
  currentUrl: string = null;
  isOnline: boolean;
  constructor(
    public title: Title,
    private router: Router,
    private activeRoute: ActivatedRoute,
    private routePartsService: RoutePartsService,
    private iconService: UILibIconService,
    private layoutService: LayoutService,
    private urlService: UrlService
  ) {
    iconService.init();
    this.isOnline = false;
  }

  ngOnInit() {
    this.changePageTitle();

    // this.themeService.applyMatTheme(this.layoutService.layoutConf.matTheme);
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.previousUrl = this.currentUrl;
        this.currentUrl = event.url;
        this.urlService.setPreviousUrl(this.previousUrl);
      });
    //pwa
    this.updateOnlineStatus();

    window.addEventListener("online", this.updateOnlineStatus.bind(this));
    window.addEventListener("offline", this.updateOnlineStatus.bind(this));
  }
  private updateOnlineStatus(): void {
    this.isOnline = window.navigator.onLine;
    console.info(`isOnline=[${this.isOnline}]`);
  }

  ngAfterViewInit() {}

  changePageTitle() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((routeChange) => {
        const routeParts = this.routePartsService.generateRouteParts(
          this.activeRoute.snapshot
        );
        if (!routeParts.length) {
          return this.title.setTitle(this.appTitle);
        }
        // Extract title from parts;
        this.pageTitle = routeParts[0].pageTitle;
        this.pageTitle += ` | ${this.appTitle}`;
        this.title.setTitle(this.pageTitle);
      });
  }
}
