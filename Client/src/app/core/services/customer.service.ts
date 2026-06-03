import { inject, Injectable } from '@angular/core';
import { ApiHttpService } from './api-http.service';
import { Customer } from '@core/models/customer.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  api = inject(ApiHttpService);

  getAllCustomers() : Observable<Customer[]>{
    return this.api.get('/customers');
  }

  getCustomerByPhone(phoneNumber: string) : Observable<Customer>{
    return this.api.get(`/customers/${phoneNumber}`);
  }

  createCustomer(data: Customer) : Observable<Customer>{
    return this.api.post('/customers', data);
  }

  updateCustomer(id: number, data: Customer){
    return this.api.put(`/customers/${id}`, data);
  }
}
