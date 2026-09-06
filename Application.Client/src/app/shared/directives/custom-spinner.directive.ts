import { AfterViewInit, Directive, ElementRef, Input } from "@angular/core";

@Directive({
  selector: "[appCustomSpinner]",
})
export class CustomSpinnerDirective implements AfterViewInit {
  @Input() color: string;

  constructor(private el: ElementRef) {}

  ngAfterViewInit() {
    if (this.color) {
      console.log(this.color);
      const element = this.el.nativeElement;
      const circle = element.querySelector("circle");
      circle.style.stroke = this.color;
    }
  }
}
