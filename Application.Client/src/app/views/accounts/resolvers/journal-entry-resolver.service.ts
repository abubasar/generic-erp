import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { JournalEntryService } from "../services/journal-entry.service";

@Injectable({
  providedIn: "root",
})
export class JournalEntryResolverService  {
  constructor(private journalEntryService: JournalEntryService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.journalEntryService.getJournalEntryById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
