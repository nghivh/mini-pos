import { inject, Injectable } from '@angular/core';
import { ApiHttpService } from './api-http.service';
import { CheckoutRequest } from '@core/models/order.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  api = inject(ApiHttpService);

  checkout(orderData: CheckoutRequest) : Observable<any>{
    return this.api.post('/orders/checkout', orderData);
  }
}
