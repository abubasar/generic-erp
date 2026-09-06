import { HttpClient } from '@angular/common/http';
import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
//import { SignalrService } from 'app/shared/services/signalr/signalr.service';
import { environment } from 'environments/environment';
import * as moment from 'moment';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html'
})
export class NotificationsComponent implements OnInit {
  @Input() notificPanel;
  notifications:any[]=[]
  constructor(private router: Router,private http:HttpClient,
    //private signalRService:SignalrService
    ) {
    // this.signalRService.startConnection();
    //  this.getNotifications();
    // this.signalRService.hubConnection.on('BroadcastMessage', () => {
    //    this.getNotifications();
    //      this.playSound();
    //     });
  }

  ngOnInit() {
    this.router.events.subscribe((routeChange) => {
        if (routeChange instanceof NavigationEnd) {
          this.notificPanel.close();
        }
    });
  }
  getNotifications(){
     this.http.get<any>(environment.apiURL+'/notification').subscribe(res=>{
         this.notifications=res.data
        })
  }
  getTime(datetime):string{
   return moment(datetime).format("LLLL"); 
   }

  clearAll(e) {
    e.preventDefault();
    this.notifications = [];
  }
   playSound() {
  const audio = new Audio(environment.baseURL+'/audio/sound.mp3');
  audio.play().catch((error) => {
    console.error("Autoplay failed:", error.message);
  });;
}
}
