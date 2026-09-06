import { Injectable } from '@angular/core';
import { AccountReportApiService } from 'app/shared/api/account/account-report-api.service';
import { Observable,map } from 'rxjs';
import { ChartOfAccount } from '../models/chart-of-account.model';

@Injectable({
  providedIn: 'root'
})
export class AccountReportService {

  constructor(private api: AccountReportApiService) {}

  getChartOfAccounts(
  ): Observable<ChartOfAccount[]> {
    return this.api
      .getChartOfAccounts()
      .pipe(map((response: ChartOfAccount[]) => response));
  }
}
