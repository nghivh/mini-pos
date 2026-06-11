import { inject, Injectable } from '@angular/core';
import { ApiHttpService } from './api-http.service';
import { ToastService } from './toast.service';
import { Observable } from 'rxjs';
import { DashboardSummary } from '@core/models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  api = inject(ApiHttpService);
  toast = inject(ToastService);

  getSummary(startDate?: string, endDate?: string) : Observable<DashboardSummary>{
    let paypload = {
      startDate: startDate,
      endDate: endDate
    }

    return this.api.get('/dashboard/summary', paypload);
  }
}
