import { HttpClient } from "@angular/common/http";
import { Component, Input, OnInit, Renderer2 } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { JwtAuthService } from "../../services/auth/jwt-auth.service";
import { LayoutService } from "../../services/layout.service";
import { ThemeService } from "../../services/theme.service";
//import { SignalrService } from 'app/shared/services/signalr/signalr.service';
import { UserProfile } from "app/shared/models/user-profile-model";
import { environment } from "environments/environment";

@Component({
  selector: "app-header-side",
  templateUrl: "./header-side.template.html",
})
export class HeaderSideComponent implements OnInit {
  @Input() notificPanel;
  public availableLangs = [
    {
      name: "EN",
      code: "en",
      flag: "us",
    },
    {
      name: "ES",
      code: "es",
      flag: "es",
    },
  ];
  currentLang = this.availableLangs[0];
  matxThemes: any[] = [];
  public layoutConf: any;
  public financialYear: string;

  constructor(
    private themeService: ThemeService,
    private layout: LayoutService,
    public translate: TranslateService,
    private renderer: Renderer2,
    public jwtAuth: JwtAuthService,
    private http: HttpClient // private signalRService:SignalrService
  ) {
    //  this.signalRService.startConnection();
    //  this.getNotificationsCount();
    // this.signalRService.hubConnection.on('BroadcastMessage', () => {
    //    this.getNotificationsCount();
    //    });
  }
  ngOnInit() {
    this.matxThemes = this.themeService.matxThemes;
    this.layoutConf = this.layout.layoutConf;
    this.translate.use(this.currentLang.code);
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYear = res.fyname;
    });
  }
  setLang(lng) {
    this.currentLang = lng;
    this.translate.use(lng.code);
  }
  changeTheme(theme) {
    this.layout.publishLayoutChange({
      matTheme: theme.name,
      footerColor: theme.footerColor,
      sidebarColor: theme.sidebarColor,
    });
  }
  toggleNotific() {
    this.notificPanel.toggle();
  }
  toggleSidenav() {
    if (this.layoutConf.sidebarStyle === "closed") {
      return this.layout.publishLayoutChange({
        sidebarStyle: "full",
      });
    }
    this.layout.publishLayoutChange({
      sidebarStyle: "closed",
    });
  }

  toggleCollapse() {
    // compact --> full
    if (this.layoutConf.sidebarStyle === "compact") {
      return this.layout.publishLayoutChange(
        {
          sidebarStyle: "full",
          sidebarCompactToggle: false,
        },
        { transitionClass: true }
      );
    }

    // * --> compact
    this.layout.publishLayoutChange(
      {
        sidebarStyle: "compact",
        sidebarCompactToggle: true,
      },
      { transitionClass: true }
    );
  }

  onSearch(e) {
    //console.log(e)
  }

  notificationCount: number = 0;
  getNotificationsCount() {
    this.http
      .get<any>(environment.apiURL + "/notification")
      .subscribe((res) => {
        this.notificationCount = res.data.length;
      });
  }
}
