import { DOCUMENT } from "@angular/common";
import {
  EventEmitter,
  Inject,
  Injectable,
  Renderer2,
  RendererFactory2,
} from "@angular/core";
import { getQueryParam } from "../helpers/url.helper";

export interface ITheme {
  name: string;
  baseColor?: string;
  isActive?: boolean;
  sidebarColor?: string;
  footerColor?: string;
}

@Injectable()
export class ThemeService {
  public onThemeChange: EventEmitter<ITheme> = new EventEmitter();

  public matxThemes: ITheme[] = [
    {
      name: "matx-navy",
      baseColor: "#f2f2f2",
      sidebarColor: "white",
      footerColor: "white",
      isActive: false,
    },
    {
      name: "matx-navy-dark",
      baseColor: "#10174c",
      sidebarColor: "slate",
      footerColor: "slate",
      isActive: false,
    },
  ];

  public activatedTheme: ITheme;
  private renderer: Renderer2;
  constructor(
    @Inject(DOCUMENT) private document: Document,
    rendererFactory: RendererFactory2
  ) {
    this.renderer = rendererFactory.createRenderer(null, null);
  }

  // Invoked in AppComponent and apply 'activatedTheme' on startup
  applyMatTheme(themeName: string) {
    this.activatedTheme = this.matxThemes.find((t) => t.name === themeName);
    this.flipActiveFlag(themeName);

    // *********** ONLY FOR DEMO **********
    this.setThemeFromQuery();
    // ************************************

    // this.changeTheme(themeName);
    this.renderer.addClass(this.document.body, themeName);
  }

  changeTheme(prevTheme, themeName: string) {
    this.renderer.removeClass(this.document.body, prevTheme);
    this.renderer.addClass(this.document.body, themeName);
    this.flipActiveFlag(themeName);
    this.onThemeChange.emit(this.activatedTheme);
  }

  flipActiveFlag(themeName: string) {
    this.matxThemes.forEach((t) => {
      t.isActive = false;
      if (t.name === themeName) {
        t.isActive = true;
        this.activatedTheme = t;
      }
    });
  }

  // *********** ONLY FOR DEMO **********
  setThemeFromQuery() {
    const themeStr = getQueryParam("theme");
    try {
      this.activatedTheme = JSON.parse(themeStr);
      console.log(this.activatedTheme);

      this.flipActiveFlag(this.activatedTheme.name);
    } catch (e) {}
  }
}
