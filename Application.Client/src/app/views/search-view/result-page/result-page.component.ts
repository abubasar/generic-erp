import { Component, OnInit, OnDestroy } from "@angular/core";
import { SearchService } from "app/shared/search/search.service";
import { Observable, Subscription } from "rxjs";
import { CountryService } from "../country.service";
import { NavigationService } from "app/shared/services/navigation.service";

@Component({
  selector: "app-result-page",
  templateUrl: "./result-page.component.html",
  styleUrls: ["./result-page.component.scss"],
})
export class ResultPageComponent implements OnInit, OnDestroy {
  public menuItems: any[];
  searchTermSub: Subscription;

  constructor(
    public searchService: SearchService,
    public countryService: CountryService,
    public navService: NavigationService
  ) {}

  ngOnInit() {
    this.searchTermSub = this.searchService.searchTerm$.subscribe(term => {
       this.navService.menuItems$.subscribe((menuItem) => {
       this.menuItems = this.getAllLastChildMenus(menuItem);
    });
     this.menuItems = this.menuItems.filter((x) =>
       x.name?.toLowerCase().includes(term?.toLowerCase())
     );
    });
   
  }
  getAllLastChildMenus(menuItems: any[], allLastChildMenus: any[] = []): any[] {
    menuItems.forEach((item) => {
      if (item.sub && item.sub.length > 0) {
        this.getAllLastChildMenus(item.sub, allLastChildMenus);
      } else {
        allLastChildMenus.push(item);
      }
    });
    return allLastChildMenus;
  }

  ngOnDestroy() {
    if (this.searchTermSub) {
      this.searchTermSub.unsubscribe();
    }
  }
}
