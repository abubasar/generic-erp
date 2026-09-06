import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Router } from "@angular/router";

interface IRoutePart {
  module: string;
  pageTitle: string;
  breadcrumb: {
    title: string;
    url: string;
  };
}

@Injectable()
export class RoutePartsService {
  public routeParts: IRoutePart[];
  constructor(private router: Router) {}

  ngOnInit() {}
  generateRouteParts(snapshot: ActivatedRouteSnapshot): IRoutePart[] {
    var routeParts = <IRoutePart[]>[];
    if (snapshot) {
      if (snapshot.firstChild) {
        routeParts = routeParts.concat(
          this.generateRouteParts(snapshot.firstChild)
        );
      }
      if (snapshot.data?.title && snapshot.data?.module) {
        // console.log(snapshot.data['title'], snapshot.url)
        routeParts.push({
          module: snapshot.data?.module,
          pageTitle: snapshot.data?.pageTitle,
          breadcrumb: {
            title: snapshot.data?.breadcrumb?.title,
            url: snapshot.data?.breadcrumb?.url,
          },
        });
      }
    }
    return routeParts;
  }
}
